import { AxiosError } from 'axios';
import { useQuery } from 'react-query';
import { Loader, Paper, Stack } from '@mantine/core';
import { getByondProfile } from '../apis/byond';
import { ByondProfile } from '../types/byondprofile';

type CKeyInfoProps = {
  byondKey: string;
};

export function CKeyInfo({ byondKey }: CKeyInfoProps) {
  const { isLoading, isError, data, error } = useQuery<ByondProfile | undefined, AxiosError>(
    ['byondProfile', byondKey],
    () => getByondProfile(byondKey)
  );

  if (isLoading) {
    return <Loader />;
  }

  if (isError) {
    return <span>Error: {error.message}</span>;
  }

  return (
    <Stack>
      <Paper shadow="xs" p="md" withBorder>
        {JSON.stringify(data)}
      </Paper>
    </Stack>
  );
}
