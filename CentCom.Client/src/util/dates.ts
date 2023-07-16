import { intlFormat } from 'date-fns';

const formatOptions = {
  year: 'numeric',
  month: 'numeric',
  day: 'numeric',
  hour: 'numeric',
  minute: 'numeric',
  timeZoneName: 'short',
} as const;

export const formatDateUTC: (date: Date) => string = (date) =>
  intlFormat(date, { timeZone: 'UTC', ...formatOptions });

export const formatDateLocal: (date: Date) => string = (date) => intlFormat(date, formatOptions);
