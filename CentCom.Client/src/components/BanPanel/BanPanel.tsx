import {
  Badge,
  Box,
  Divider,
  Flex,
  Grid,
  Group,
  Paper,
  Text,
  useMantineTheme,
} from '@mantine/core';
import { Ban } from '../../types/ban';
import { useStyles } from './BanPanel.style';
import { getBanColor } from './BanUtils';
import { BanTimeline } from './BanTimeline';
import { useMediaQuery } from '@mantine/hooks';
import React from 'react';

type BanPanelProps = {
  ban: Ban;
};

export function BanPanel({ ban }: BanPanelProps) {
  const banColor = getBanColor(ban);
  const { classes } = useStyles({ color: banColor });
  const theme = useMantineTheme();
  const verticalLayout = useMediaQuery(`(max-width: ${theme.breakpoints.sm})`);

  const jobs = ban.jobs && (
    <>
      <Divider my="md" label="Jobs" labelPosition="center" variant="dashed" />
      <Flex gap={5} rowGap={5} wrap="wrap">
        {ban.jobs.map((job) => (
          <Badge key={job}>{job}</Badge>
        ))}
      </Flex>
    </>
  );

  return (
    <Paper className={classes.root} shadow="xs" p="lg" radius="md" withBorder>
      <Grid>
        <Grid.Col span={12} sm={8}>
          <Group align="center">
            <Badge size="lg" radius="xs" p="xs" color={banColor}>
              {ban.active ? 'Active' : ban.unbannedBy !== undefined ? 'Unbanned' : 'Expired'}
            </Badge>
            <Text ta="center" fz="lg" weight={700} span>
              {ban.sourceName} &gt; {ban.type}
            </Text>
          </Group>
          <Divider
            my="md"
            label={
              ban.banID && (
                <Badge radius="xs" p="xs">
                  Ban {ban.banID}
                </Badge>
              )
            }
            labelPosition="center"
          />
          <Box>
            <Text className={classes.banReason}>{ban.reason}</Text>
            {ban.jobs && jobs}
          </Box>
        </Grid.Col>
        <Grid.Col span={12} sm={4}>
          {verticalLayout && (
            <Divider my="md" label="Details" labelPosition="center" variant="solid" />
          )}
          <BanTimeline ban={ban} />
        </Grid.Col>
      </Grid>
    </Paper>
  );
}

export const MemoizedBanPanel = React.memo(BanPanel);
