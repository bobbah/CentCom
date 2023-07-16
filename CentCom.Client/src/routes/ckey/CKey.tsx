import { useParams } from 'react-router';
import { BanStack } from '../../components/BanStack/BanSearchResults';

export function CKey() {
  const { ckey } = useParams();

  return (
    <>
      <BanStack byondKey={ckey!} />
    </>
  );
}
