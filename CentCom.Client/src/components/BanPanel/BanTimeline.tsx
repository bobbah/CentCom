import { formatDistance } from 'date-fns';
import { Timeline, Tooltip, Text } from '@mantine/core';
import { IconGavel, IconUserCheck, IconAlarm, IconConfetti, IconX } from '@tabler/icons-react';
import { Ban } from '../../types/ban';
import { formatDateLocal, formatDateUTC } from '../../util/dates';
import { getBanColor } from './BanUtils';

type BanTimelineProps = {
  ban: Ban;
};

export const BanTimeline = ({ ban }: BanTimelineProps) => {
  const color = getBanColor(ban);
  return (
    <Timeline active={ban.active ? 0 : 1} bulletSize={24} lineWidth={2} color={color}>
      <Timeline.Item
        bullet={<IconGavel size={16} />}
        title="Ban placed"
        lineVariant={ban.expires !== undefined ? 'solid' : 'dashed'}>
        <Text color="dimmed" size="sm">
          Ban was placed by{' '}
          <Text span fw={400} c={color} inherit>
            {ban.bannedBy}
          </Text>
          .
        </Text>
        <Tooltip label={formatDateLocal(ban.bannedOn)}>
          <Text size="xs" mt={4} span>
            {formatDateUTC(ban.bannedOn)}
          </Text>
        </Tooltip>
      </Timeline.Item>
      {ban.expires !== undefined ? (
        (ban.unbannedBy !== undefined && (
          <Timeline.Item bullet={<IconUserCheck size={16} />} title="Unbanned">
            <Text color="dimmed" size="sm">
              Ban was lifted by{' '}
              <Text span fw={400} c={color} inherit>
                {ban.unbannedBy}
              </Text>{' '}
              after {formatDistance(ban.bannedOn, ban.expires)}.
            </Text>
            <Tooltip label={formatDateLocal(ban.expires)}>
              <Text size="xs" mt={4} span>
                {formatDateUTC(ban.expires)}
              </Text>
            </Tooltip>
          </Timeline.Item>
        )) || (
          <Timeline.Item
            bullet={ban.active ? <IconAlarm size={16} /> : <IconConfetti size={16} />}
            title={ban.active ? 'Ban expires' : 'Ban expired'}>
            <Text color="dimmed" size="sm">
              {ban.active
                ? `Ban should expire after ${formatDistance(ban.bannedOn, ban.expires)}.`
                : `Ban expired after ${formatDistance(ban.bannedOn, ban.expires)}.`}
            </Text>
            <Tooltip label={formatDateLocal(ban.expires)}>
              <Text size="xs" mt={4} span>
                {formatDateUTC(ban.expires)}
              </Text>
            </Tooltip>
          </Timeline.Item>
        )
      ) : (
        <Timeline.Item bullet={<IconX color="red" size={16} />} title="Permanent">
          <Text color="dimmed" size="sm">
            Without intervention this ban will never expire.
          </Text>
        </Timeline.Item>
      )}
    </Timeline>
  );
};
