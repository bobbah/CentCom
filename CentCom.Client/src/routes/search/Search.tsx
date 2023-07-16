import { Box, Button, TextInput } from '@mantine/core';
import { hasLength, useForm } from '@mantine/form';
import { useState } from 'react';
import { BanStack } from '../../components/BanStack/BanSearchResults';

type SearchParameters = {
  ckey: string;
};

export function Search() {
  const [searchValues, setSearchValues] = useState<SearchParameters>();

  const form = useForm<SearchParameters>({
    initialValues: {
      ckey: '',
    },
    validate: {
      ckey: hasLength({ min: 1, max: 32 }, 'Banned BYOND key must be 1-32 characters long.'),
    },
  });

  return (
    <Box>
      <form onSubmit={form.onSubmit((values) => setSearchValues(values))}>
        <TextInput label="Banned User Key" {...form.getInputProps('ckey')} />
        <Button type="submit" mt="md">
          Submit
        </Button>
      </form>
      {searchValues?.ckey && <BanStack byondKey={searchValues.ckey} />}
    </Box>
  );
}
