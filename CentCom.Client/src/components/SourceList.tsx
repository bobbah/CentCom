import { AxiosError } from 'axios';
import { useQuery } from 'react-query';
import { listSources } from '../apis/centcom';
import { Source } from '../types/source';
import { Loader, ScrollArea, Table } from '@mantine/core';

export function SourceList() {
  const { isLoading, isError, data, error } = useQuery<Source[], AxiosError>(
    'sources',
    listSources
  );

  if (isLoading) {
    return <Loader />;
  }

  if (isError) {
    return <span>Error: {error.message}</span>;
  }

  const rows = data?.map((source) => {
    return (
      <tr key={source.id}>
        <td>{source.id}</td>
        <td>{source.name}</td>
        <td>{source.roleplayLevel}</td>
      </tr>
    );
  });

  return (
    <ScrollArea>
      <Table>
        <thead>
          <tr>
            <th>Source ID</th>
            <th>Name</th>
            <th>Roleplay Level</th>
          </tr>
        </thead>
        <tbody>{rows}</tbody>
      </Table>
    </ScrollArea>
  );
}
