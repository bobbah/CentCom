import {
  CSSObject,
  MantineNumberSize,
  MantineTheme,
  createStyles,
  getSize,
  rem,
} from '@mantine/core';

export const MAIN_PADDING = 'md';
export const NAVBAR_WIDTH = rem(260);
export const NAVBAR_BREAKPOINT = 'sm';
export const FOOTER_HEIGHT = rem(100);
export const HEADER_HEIGHT = rem(60);

function getPositionStyles(theme: MantineTheme): CSSObject {
  const padding = getSize({ size: 'md', sizes: theme.spacing });

  return {
    minHeight: '100vh',
    paddingTop: `calc(var(--mantine-header-height, 0px) + ${padding})`,
    paddingBottom: `calc(var(--mantine-footer-height, 0px) + ${padding})`,
    paddingLeft: `calc(var(--mantine-navbar-width, 0px) + ${padding})`,
    paddingRight: `calc(var(--mantine-aside-width, 0px) + ${padding})`,
  };
}

export const useStyles = createStyles((theme) => ({
  root: {
    boxSizing: 'border-box',
  },

  body: {
    display: 'flex',
    boxSizing: 'border-box',
  },

  main: {
    flex: 1,
    width: '100vw',
    boxSizing: 'border-box',
    ...getPositionStyles(theme),
  },
}));
