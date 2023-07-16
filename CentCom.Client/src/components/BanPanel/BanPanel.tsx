import {
  Badge,
  Box,
  Divider,
  Flex,
  Grid,
  Group,
  Paper,
  ScrollArea,
  Text,
  rem,
} from '@mantine/core';
import { Ban } from '../../types/ban';
import { useStyles } from './BanPanel.style';
import { getBanColor } from './BanUtils';
import { BanTimeline } from './BanTimeline';

type BanPanelProps = {
  ban: Ban;
};

export function BanPanel({ ban }: BanPanelProps) {
  const banColor = getBanColor(ban);
  const { classes } = useStyles({ color: banColor });

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
      <Group position="center" align="center">
        <Text ta="center" fz="lg" weight={700} span>
          {ban.sourceName} &gt; {ban.type}
        </Text>
        <Badge size="lg" radius="xs" p="xs" color={banColor}>
          {ban.active ? 'Active' : ban.unbannedBy !== undefined ? 'Unbanned' : 'Expired'}
        </Badge>
      </Group>
      <Grid>
        <Grid.Col span={12} sm={8} order={0}>
          <Divider
            my="md"
            label={
              (ban.banID && (
                <Badge radius="xs" p="xs" px={rem(5)}>
                  Ban {ban.banID}
                </Badge>
              )) ||
              'Reason'
            }
            labelPosition="center"
            labelProps={{ h: rem(22) }}
          />
          <Box>
            <Text className={classes.banReason}>{ban.reason}</Text>
            {ban.jobs && !ban.banAttributes && jobs}
          </Box>
        </Grid.Col>
        {ban.jobs && ban.banAttributes && (
          <Grid.Col span={12} sm={8} order={1} orderSm={2}>
            {jobs}
          </Grid.Col>
        )}
        <Grid.Col span={12} sm={4} order={2} orderSm={1}>
          <Divider my="md" label="Details" labelPosition="center" labelProps={{ h: rem(22) }} />
          <BanTimeline ban={ban} />
        </Grid.Col>
        {ban.banAttributes && (
          <Grid.Col span={12} sm={4} order={3} offsetSm={ban.jobs ? 0 : 8}>
            <Divider my="md" label="Attributes" labelPosition="center" variant="dashed" />
            <Flex gap={5} rowGap={5} wrap="wrap">
              {ban.banAttributes.map((attr) => (
                <Badge key={attr}>{attr}</Badge>
              ))}
            </Flex>
          </Grid.Col>
        )}
      </Grid>
    </Paper>
  );
}
