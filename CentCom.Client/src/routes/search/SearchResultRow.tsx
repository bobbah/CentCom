import {
  Avatar,
  Badge,
  Box,
  Group,
  Paper,
  Stack,
  Text,
  Tooltip,
  UnstyledButton,
} from '@mantine/core';
import { KeySummary } from '../../types/keysummary';
import { Link } from 'react-router-dom';
import { formatDateLocal, formatDateUTC } from '../../util/dates';
import { formatDistance } from 'date-fns';
import { RandomAvatar } from '../../components/RandomAvatar';
import React from 'react';
import { useStyles } from './SearchResultRow.styles';

type SearchResultRowProps = {
  data: KeySummary;
};

export const SearchResultRow = React.memo(({ data }: SearchResultRowProps) => {
  const { classes } = useStyles();

  return (
    <UnstyledButton component={Link} to={`/ckey/${data.cKey}`}>
      <Paper shadow="xs" p="md" radius="md" withBorder className={classes.root}>
        <Group position="apart">
          <Group>
            <RandomAvatar size="md" value={data.cKey} />
            <Text size="lg">{data.cKey}</Text>
          </Group>
          <Group>
            {data.serverBans ? <Badge size="sm">{data.serverBans} Server Bans</Badge> : null}
            {data.jobBans ? <Badge size="sm">{data.jobBans} Job Bans</Badge> : null}
            <Tooltip label={formatDateLocal(data.latestBan)}>
              <Text size="xs">
                Last banned {formatDistance(data.latestBan, new Date(), { addSuffix: true })}
              </Text>
            </Tooltip>
          </Group>
        </Group>
      </Paper>
    </UnstyledButton>
  );
});
