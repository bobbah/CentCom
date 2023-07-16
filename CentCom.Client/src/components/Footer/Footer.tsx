import { Flex, Container, Text, Box } from '@mantine/core';
import useStyles from './Footer.styles';

interface FooterProps {
  withNavbar?: boolean;
}

export function Footer({ withNavbar }: FooterProps) {
  const { classes, cx } = useStyles();

  return (
    <>
      <div className={classes.spacer} />
      <div className={classes.wrapper}>
        <Container
          size={1100}
          sx={{
            display: 'flex',
            height: '100%',
            justifyContent: 'center',
            alignItems: 'center',
          }}>
          <Box>
            <Text size="xs" ta="center">
              All times listed are UTC unless otherwise stated. All content is provided without any
              guarantee, explicit or implied, of accuracy or permanent access.
            </Text>
            <Text size="xs" ta="center" mt="xs">
              Copyright © 2023 MelonMesa and CentCom Contributors
            </Text>
          </Box>
        </Container>
      </div>
    </>
  );
}
