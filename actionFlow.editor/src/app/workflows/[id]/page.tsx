import { useRouter } from 'next/router'
import ClientPage from './ClientPage';

export default async function Workflows() {
    const router = useRouter()
  return <ClientPage />
}
