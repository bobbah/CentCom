import { MantineColor } from '@mantine/core';
import { Ban } from '../../types/ban';

export const getBanColor: (ban: Ban) => MantineColor = (ban) => {
  if (ban.active) return 'red';
  if (ban.unbannedBy !== undefined) return 'blue';
  return 'green';
};
