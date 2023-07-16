import { AxiosError } from 'axios';
import { useQuery } from 'react-query';
import { banSearchByKey } from '../../apis/centcom';
import { Ban } from '../../types/ban';
import { Loader, Paper, Progress, SimpleGrid, Stack, Text } from '@mantine/core';
import { useEffect, useState } from 'react';
import { BanPanel } from '../BanPanel/BanPanel';

type BanStackProps = {
  byondKey: string;
};

type StatsSegment = {
  label: string;
  value: number;
  color: string;
  tooltip: string;
};

type StatsConstructor = {
  labelFn: (count: number) => string;
  color: string;
  tooltipFn: (count: number) => string;
};

export function BanStack({ byondKey }: BanStackProps) {
  const [banSections, setBanSections] = useState<StatsSegment[]>([]);
  const { isLoading, isError, data, error } = useQuery<Ban[], AxiosError>(
    ['banSearch', byondKey],
    () => banSearchByKey(byondKey)
  );

  // Count active / inactive bans
  useEffect(() => {
    if (isLoading) return;

    const newSegments: StatsSegment[] = [];
    const stats: { [id: string]: { [id: string]: number } } = {
      permanent: {
        server: 0,
        job: 0,
      },
      temporary: {
        server: 0,
        job: 0,
      },
      inactive: {
        server: 0,
        job: 0,
      },
    };

    // Count bans
    let totalBans = 0;
    for (const ban of data || []) {
      stats[ban.active ? (ban.expires ? 'temporary' : 'permanent') : 'inactive'][
        ban.type === 'Server' ? 'server' : 'job'
      ] += 1;
      totalBans++;
    }

    // Develop progress bars
    if (totalBans === 0) return;

    const statMapping: { [index: string]: StatsConstructor } = {
      permanent: {
        labelFn: (count) => `${count} Perma`,
        color: 'red',
        tooltipFn: (count) => `${count} permanent bans`,
      },
      temporary: {
        labelFn: (count) => `${count} Temp`,
        color: 'yellow',
        tooltipFn: (count) => `${count} temporary bans`,
      },
      inactive: {
        labelFn: (count) => `${count} Inactive`,
        color: 'gray',
        tooltipFn: (count) => `${count} inactive bans`,
      },
    };

    for (const type of ['permanent', 'temporary', 'inactive']) {
      const count = stats[type].server + stats[type].job;
      if (count > 0)
        newSegments.push({
          value: (count / totalBans) * 100,
          color: statMapping[type].color,
          label: statMapping[type].labelFn(count),
          tooltip: statMapping[type].tooltipFn(count),
        });
    }

    setBanSections(newSegments);
  }, [data, isLoading]);

  if (isLoading) {
    return <Loader />;
  }

  if (isError) {
    return <span>Error: {error.message}</span>;
  }

  return (
    <Stack spacing="sm">
      <SimpleGrid cols={3} breakpoints={[{ maxWidth: 'xs', cols: 1 }]} spacing="sm">
        <Paper shadow="xs" p="md" withBorder>
          <Text ta="center" fz="lg" weight={500} mt="md">
            {byondKey}
          </Text>
        </Paper>
        <Paper shadow="xs" p="md" withBorder>
          <Progress size={24} sections={banSections} />
        </Paper>
        <Paper shadow="xs" p="md" withBorder>
          <Text>Some other info here...</Text>
        </Paper>
      </SimpleGrid>
      {data?.map((ban) => {
        return <BanPanel key={ban.id} ban={ban} />;
      })}
    </Stack>
  );
}
