import axios from 'axios';
import { ByondProfile } from '../types/byondprofile';

export const ApiURL = 'https://secure.byond.com';

const deconstructByondResponse: (key: string) => ByondProfile = (text) => {
  const kvpRegex = /^\s*(?<key>[\w]+) = "?(?<value>.+?)"?$/gm;
  const matches = text.matchAll(kvpRegex);
  const rawResult: { [key: string]: string } = {};
  for (const match of matches) {
    rawResult[match.groups!['key']] = match.groups!['value'];
  }
  return {
    key: rawResult['key'],
    ckey: rawResult['ckey'],
    inactive: false,
    gender: rawResult['gender'],
    joined: new Date(rawResult['joined']),
    online: rawResult['online'] === '1',
    member: rawResult['is_member'] === '1',
    icon: rawResult['icon'],
    largeIcon: rawResult['large_icon'],
  } as ByondProfile;
};

// Note to self: you can't actually use this currently because of CORS policies or lack thereof on BYOND's servers
export const getByondProfile = (key: string) => {
  const ckeyCleanupRegex = /[^a-z0-9]/;
  const ckey = key.toLowerCase().replace(ckeyCleanupRegex, '');
  const regex =
    />User (?<key>[\w.-][\w. -]{0,28}[\w.-]?) (?<discriminator>not found|is not active).</;
  return axios
    .get<ByondProfile | undefined>(`${ApiURL}/members/${ckey}`, {
      params: { format: 'text' },
      transformResponse: (res, header) => {
        // If we get a HTML response this implies a unknown or inactive user
        const resString = res as string;
        if (resString === undefined) return undefined;
        if (
          header['Content-Type'] !== undefined &&
          header['Content-Type']?.toString().startsWith('text/html')
        ) {
          const match = regex.exec(resString);
          if (match === null || match[0] === null) return undefined; // Didn't match
          const exists = match.groups!['discriminator'].toString() !== 'not found';
          return exists
            ? ({ ckey: ckey, key: match.groups!['key'].toString(), inactive: true } as ByondProfile)
            : undefined;
        } else if (
          header['Content-Type'] !== undefined &&
          header['Content-Type']?.toString().startsWith('text/plain')
        ) {
          return deconstructByondResponse(resString);
        } else return undefined;
      },
    })
    .then((resp) => resp.data);
};
