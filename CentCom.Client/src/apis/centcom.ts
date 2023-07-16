import axios from 'axios';
import { Ban } from '../types/ban';
import { Source } from '../types/source';

export const ApiURL = 'https://centcom.melonmesa.com';

function dateHandler<T>(data: { [key: string]: any }, fields: string[]): T {
  if ((data as Array<{ [key: string]: any }>) !== undefined) {
    for (const item of data as Array<{ [key: string]: any }>) {
      for (const field of fields) {
        if (item[field] !== undefined && (item[field] as string) !== undefined)
          item[field] = new Date(item[field] as string);
      }
    }
  } else {
    for (const field of fields) {
      if (data[field] !== undefined && (data[field] as string) !== undefined)
        data[field] = new Date(data[field] as string);
    }
  }

  return data as T;
}

export const banSearchByKey = (key: string, onlyActive?: boolean, source?: number) => {
  const params: { onlyActive?: boolean; source?: number } = { onlyActive, source };
  return axios
    .get<Ban[]>(`${ApiURL}/ban/search/${key}`, { params: params })
    .then((resp) => dateHandler<Ban[]>(resp.data, ['bannedOn', 'expires']));
};

export const listSources = () =>
  axios.get<Source[]>(`${ApiURL}/source/list`).then((resp) => resp.data);
