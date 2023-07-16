import {
  ActionIcon,
  Burger,
  Container,
  Group,
  Header,
  Paper,
  Transition,
  useMantineColorScheme,
} from '@mantine/core';
import { Logo } from '../Logo';
import { NavLink } from 'react-router-dom';
import { useDisclosure } from '@mantine/hooks';
import { HEADER_HEIGHT } from '../Layout/Layout.styles';
import useStyles from './CCHeader.styles';
import { Sun, MoonStars } from 'tabler-icons-react';
import { ReactNode } from 'react';

type CCHeaderProps = {
  links: { link: string; label: string; icon?: ReactNode }[];
};

export function CCHeader({ links }: CCHeaderProps) {
  // eslint-disable-next-line @typescript-eslint/unbound-method
  const { colorScheme, toggleColorScheme } = useMantineColorScheme();
  const [opened, { toggle }] = useDisclosure(false);
  const { classes } = useStyles();
  const dark = colorScheme === 'dark';

  const items = links.map((link) => (
    <NavLink key={link.label} to={link.link} className={classes.link}>
      {link.icon}
      {link.label}
    </NavLink>
  ));

  return (
    <Header height={HEADER_HEIGHT} className={classes.root}>
      <Container size={1100} className={classes.header}>
        <Logo />
        <Group spacing={5} className={classes.links}>
          {items}
          <ActionIcon
            color={dark ? 'yellow' : 'blue'}
            onClick={() => toggleColorScheme()}
            title="Toggle dark mode">
            {dark ? <Sun size="1rem" /> : <MoonStars size="1rem" />}
          </ActionIcon>
        </Group>
        <Burger opened={opened} onClick={toggle} className={classes.burger} size="sm" />

        <Transition transition="pop-top-right" duration={200} mounted={opened}>
          {(styles) => (
            <Paper className={classes.dropdown} withBorder style={styles}>
              {items}
              <a
                className={classes.link}
                color={dark ? 'yellow' : 'blue'}
                onClick={() => toggleColorScheme()}
                title="Toggle dark mode">
                {dark ? 'enter the light' : 'embrace the dark'}
              </a>
            </Paper>
          )}
        </Transition>
      </Container>
    </Header>
  );
}
