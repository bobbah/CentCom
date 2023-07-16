import { Text, Title } from '@mantine/core';
import { SourceList } from '../../components/SourceList';

export function Home() {
  return (
    <>
      <Title order={1}>CentCom Ban Database</Title>
      <Text>This is where we'd put some interesting information on CentCom itself</Text>
      <SourceList />
    </>
  );
}
