import { createStyles } from '@mantine/core';
import { FOOTER_HEIGHT, MAIN_PADDING } from '../Layout/Layout.styles';

export default createStyles((theme) => ({
  spacer: {
    height: FOOTER_HEIGHT,
  },
  wrapper: {
    backgroundColor: theme.colorScheme === 'dark' ? theme.colors.dark[8] : theme.colors.gray[0],
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    height: FOOTER_HEIGHT,
    margin: `calc(${theme.spacing[MAIN_PADDING]} * -1)`,
    marginTop: 0,
  },
  inner: {
    display: 'flex',
    flexDirection: 'column',
  },
}));
