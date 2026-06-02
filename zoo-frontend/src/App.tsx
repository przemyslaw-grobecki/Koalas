import { useState } from 'react';
import { AppBar, Toolbar, Typography, Container, Tabs, Tab, Box } from '@mui/material';
import KoalaPanel from './components/KoalaPanel';
import BambooPanel from './components/BambooPanel';
import StatsPanel from './components/StatsPanel';

function App() {
  const [tab, setTab] = useState(0);

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.100' }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Zoo Management Dashboard
          </Typography>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg" sx={{ mt: 3 }}>
        <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
          <Tab label="Koalas" />
          <Tab label="Bamboo" />
          <Tab label="Statistics" />
        </Tabs>
        {tab === 0 && <KoalaPanel />}
        {tab === 1 && <BambooPanel />}
        {tab === 2 && <StatsPanel />}
      </Container>
    </Box>
  );
}

export default App;