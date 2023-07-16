import React from 'react';
import ReactDOM from 'react-dom/client';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import { AppFrame } from './AppFrame.tsx';
import { Home } from './routes/home/Home.tsx';
import { Search } from './routes/search/Search.tsx';
import { ErrorPage } from './components/ErrorPage.tsx';
import { Layout } from './components/Layout/Layout.tsx';
import { NotFoundPage } from './components/NotFoundPage/NotFoundPage.tsx';
import { QueryClient, QueryClientProvider } from 'react-query';
import { CKey } from './routes/ckey/CKey.tsx';

const router = createBrowserRouter([
  {
    path: '',
    element: <Layout />,
    children: [
      {
        errorElement: <ErrorPage />,
        children: [
          {
            path: '/',
            index: true,
            element: <Home />,
          },
          {
            path: 'search',
            element: <Search />,
          },
          {
            path: 'ckey/:ckey',
            element: <CKey />,
          },
          {
            path: '*',
            element: <NotFoundPage />,
          },
        ],
      },
    ],
  },
]);

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <AppFrame>
        <RouterProvider router={router} />
      </AppFrame>
    </QueryClientProvider>
  </React.StrictMode>
);
