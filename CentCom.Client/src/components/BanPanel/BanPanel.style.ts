import { MantineColor, createStyles, rem } from '@mantine/core';

type BanPanelStyleParams = {
  color: MantineColor;
};

export const useStyles = createStyles((theme, { color }: BanPanelStyleParams) => {
  const colors = theme.fn.variant({ variant: 'filled', color });

  return {
    root: {
      position: 'relative',
      paddingLeft: rem(22),
      overflow: 'hidden',
      backgroundColor: theme.colorScheme === 'dark' ? theme.colors.dark[6] : theme.colors.gray[0],

      '&::before': {
        content: '""',
        display: 'block',
        position: 'absolute',
        width: rem(6),
        top: 0,
        bottom: 0,
        left: 0,
        backgroundColor: colors.background,
      },
    },

    banReason: {
      overflowWrap: 'break-word',
    },
  };
});
