import { Alert, Text } from '@mantine/core';
import { AxiosError } from 'axios';
import { Problem } from '../types/problem';
import { IconAlertCircle } from '@tabler/icons-react';
import { ReactNode } from 'react';

type ErrorAlertProps = {
  error: AxiosError<Problem>;
};

const getErrorContent = (error: AxiosError<Problem>): { title: string; content: ReactNode } => {
  if (error.code === 'ERR_NETWORK')
    return {
      title: 'Cannot Connect to CentCom API',
      content:
        'Unable to connect to CentCom API. Check your network connectivity, or try again later.',
    };
  if (!error.response?.data)
    return {
      title: 'CentCom API Encountered An Issue',
      content:
        'An error occurred, but the CentCom API did not send any details. Check your browser console for more details, or try again later.',
    };
  return {
    title: `CentCom API Error: ${error.response.data.title} (${error.response.data.status})`,
    content: (
      <div>
        <Text size="sm">{error.response.data.detail}</Text>
        <Text size="xs" opacity={0.65} mt="xs">
          Trace ID: {error.response.data.traceId}
        </Text>
      </div>
    ),
  };
};

export const ErrorAlert = ({ error }: ErrorAlertProps) => {
  const errorContent = getErrorContent(error);
  return (
    <Alert
      icon={<IconAlertCircle size="1rem" />}
      title={errorContent.title}
      color="red"
      mb="md"
      variant="outline">
      {errorContent.content}
    </Alert>
  );
};
