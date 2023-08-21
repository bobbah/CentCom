import {
  Box,
  Loader,
  TextInput,
  ActionIcon,
  useMantineTheme,
  Stack,
  Skeleton,
  Text,
  Center,
} from '@mantine/core';
import { hasLength, useForm } from '@mantine/form';
import { ReactNode, useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { KeySummary } from '../../types/keysummary';
import { AxiosError } from 'axios';
import { searchPlayers } from '../../apis/centcom';
import { Problem } from '../../types/problem';
import { IconArrowLeft, IconArrowRight, IconSearch } from '@tabler/icons-react';
import { useSearchParams } from 'react-router-dom';
import { ErrorAlert } from '../../components/ErrorAlert';
import { SearchResultRow } from './SearchResultRow';

type SearchParameters = {
  ckey: string;
};

export function Search() {
  const theme = useMantineTheme();
  const [searchParams, setSearchParams] = useSearchParams();
  const [submittedSearch, setSubmittedSearch] = useState<SearchParameters>({
    ckey: searchParams.get('query') || '',
  });
  const form = useForm<SearchParameters>({
    initialValues: submittedSearch,
    validate: {
      ckey: hasLength({ min: 3, max: 32 }, 'Banned BYOND key must be 3-32 characters long.'),
    },
    validateInputOnChange: true,
  });
  const {
    data: searchResult,
    error: searchError,
    isFetching: searchLoading,
  } = useQuery<KeySummary[], AxiosError<Problem>>({
    queryKey: ['playerSearch', submittedSearch.ckey],
    queryFn: () => {
      const query = submittedSearch.ckey;
      if (query === null || query === '') return [];
      return searchPlayers(query);
    },
    retry: (failureCount, error) => error.code !== 'ERR_BAD_REQUEST' && failureCount < 3,
    staleTime: 10000,
  });

  useEffect(() => {
    const paramsData: SearchParameters = {
      ckey: searchParams.get('query') || '',
    };
    if (paramsData.ckey !== submittedSearch.ckey) {
      console.log(paramsData, submittedSearch);
      form.setValues(paramsData);
      setSubmittedSearch(form.values);
    }
  }, [searchParams, form, submittedSearch]);

  const searchContent = (): ReactNode => {
    if (searchLoading)
      return (
        <Center h="25vh">
          <Loader variant="dots" />
        </Center>
      );
    if (submittedSearch.ckey === '' || searchError) return null;
    if (!searchResult || searchResult.length === 0)
      return (
        <Center>
          <Text>No results found...</Text>
        </Center>
      );
    return (
      <Stack>
        {searchResult.map((r) => (
          <SearchResultRow data={r} />
        ))}
      </Stack>
    );
  };

  return (
    <Box>
      {searchError ? <ErrorAlert error={searchError} /> : null}
      <Box mb="md">
        <form
          onSubmit={form.onSubmit((values) => {
            setSubmittedSearch(values);
            setSearchParams({ query: values.ckey });
          })}>
          <TextInput
            icon={<IconSearch size="1.1rem" stroke={1.5} />}
            size="md"
            rightSection={
              <ActionIcon
                size={32}
                color={theme.primaryColor}
                variant="filled"
                disabled={searchLoading}
                type="submit">
                {searchLoading ? (
                  <Loader size="1.1rem" stroke="1.5" />
                ) : theme.dir === 'ltr' ? (
                  <IconArrowRight size="1.1rem" stroke="1.5" />
                ) : (
                  <IconArrowLeft size="1.1rem" stroke="1.5" />
                )}
              </ActionIcon>
            }
            rightSectionWidth={42}
            placeholder="Search for a player..."
            {...form.getInputProps('ckey')}
          />
        </form>
      </Box>
      {searchContent()}
    </Box>
  );
}
