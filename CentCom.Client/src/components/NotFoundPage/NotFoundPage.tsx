import { Title, Text, Container, Group, Button } from '@mantine/core';
import { useStyles } from './NotFoundPage.style';
import { Link } from 'react-router-dom';

export function NotFoundPage() {
  const { classes } = useStyles();

  return (
    <Container className={classes.root}>
      <div className={classes.label}>404</div>
      <Title className={classes.title}>Where exactly are we...</Title>
      <Text color="dimmed" size="lg" align="center" className={classes.description}>
        It would appear you've stumbled upon a page that doesn't exist, or no longer exists. Check
        the URL for errors.
      </Text>
      <Group position="center">
        <Link to="/">
          <Button variant="subtle" size="md">
            Return to home
          </Button>
        </Link>
      </Group>
    </Container>
  );
}
