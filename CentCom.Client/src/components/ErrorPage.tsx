import { Title, Text, Code } from '@mantine/core';
import { isRouteErrorResponse, useRouteError } from 'react-router-dom';

export function ErrorPage() {
  const error = useRouteError();

  return (
    <>
      <Title>Uh oh! Looks like something went wrong.</Title>
      <Text mt="xl">We're looking into this one, we'll get back to you.</Text>
      {isRouteErrorResponse(error) ? error.error?.message || error.statusText : 'Unknown error message'}
    </>
  );
}
