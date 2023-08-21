import { CCHeader } from '../Header/CCHeader';
import { Footer } from '../Footer/Footer';
import { Box, Container } from '@mantine/core';
import { useStyles } from './Layout.styles';
import { Outlet } from 'react-router-dom';
import { IconExternalLink } from '@tabler/icons-react';

export function Layout() {
  const { classes } = useStyles();
  const links = [
    {
      link: '/',
      label: 'home',
    },
    {
      link: '/search',
      label: 'search',
    },
    {
      link: '/faq',
      label: 'faq',
    },
    {
      link: 'https://centcom.melonmesa.com/swagger/index.html',
      label: 'documentation',
      icon: <IconExternalLink size="1rem" />,
    },
  ];

  return (
    <Box className={classes.root}>
      <CCHeader links={links} />
      <div className={classes.body}>
        <main className={classes.main}>
          <Box sx={() => ({ height: '100%', position: 'relative' })}>
            <Container size={1100}>
              <Outlet />
            </Container>
            <Footer />
          </Box>
        </main>
      </div>
    </Box>
  );
}
