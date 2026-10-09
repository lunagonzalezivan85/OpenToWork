import { Navigate, Route } from 'react-router-dom';
import {
  IonApp, IonIcon, IonLabel, IonRouterOutlet, IonSpinner,
  IonTabBar, IonTabButton, IonTabs, setupIonicReact,
} from '@ionic/react';
import { IonReactRouter } from '@ionic/react-router';
import {
  briefcaseOutline, documentTextOutline, mailOutline,
  personOutline, peopleOutline,
} from 'ionicons/icons';
import { AuthProvider, useAuth } from './auth/AuthContext';
import AppMenu from './components/AppMenu';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Vacancies from './pages/candidate/Vacancies';
import Applications from './pages/candidate/Applications';
import MyProcess from './pages/candidate/MyProcess';
import MyVacancies from './pages/company/MyVacancies';
import CandidateSearch from './pages/company/CandidateSearch';
import Deliveries from './pages/company/Deliveries';
import Messages from './pages/Messages';
import News from './pages/News';
import Profile from './pages/Profile';

import '@ionic/react/css/core.css';
import '@ionic/react/css/normalize.css';
import '@ionic/react/css/structure.css';
import '@ionic/react/css/typography.css';
import '@ionic/react/css/padding.css';
import '@ionic/react/css/flex-utils.css';
import '@ionic/react/css/palettes/dark.class.css';
import './theme/variables.css';

setupIonicReact();

const Shell: React.FC = () => {
  const { user, loading } = useAuth();

  if (loading)
    return (
      <div style={{ height: '100%', display: 'grid', placeItems: 'center' }}>
        <IonSpinner />
      </div>
    );

  if (!user) return <Login />;

  const isCompany = user.primaryRole === 1;
  const home = isCompany ? '/company/dashboard' : '/candidate/dashboard';

  return (
    <>
      {/* Menu lateral con TODAS las secciones del rol - la tab bar solo lleva lo esencial */}
      <AppMenu />
      <div id="main" style={{ height: '100%' }}>
      <IonTabs>
        <IonRouterOutlet>
          {/* Candidato */}
          <Route path="/candidate/dashboard" element={<Dashboard />} />
          <Route path="/candidate/vacancies" element={<Vacancies />} />
          <Route path="/candidate/applications" element={<Applications />} />
          <Route path="/candidate/process" element={<MyProcess />} />
          {/* Empresa */}
          <Route path="/company/dashboard" element={<Dashboard />} />
          <Route path="/company/vacancies" element={<MyVacancies />} />
          <Route path="/company/candidates" element={<CandidateSearch />} />
          <Route path="/company/deliveries" element={<Deliveries />} />
          {/* Compartidas */}
          <Route path="/messages" element={<Messages />} />
          <Route path="/news" element={<News />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/" element={<Navigate to={home} replace />} />
        </IonRouterOutlet>

        <IonTabBar slot="bottom">
          {/* OJO: los IonTabButton deben ser hijos DIRECTOS del IonTabBar - un
              Fragment aqui hace que no se registren y no se rendericen. */}
          {isCompany
            ? [
                <IonTabButton key="cv" tab="vacancies" href="/company/vacancies">
                  <IonIcon icon={briefcaseOutline} /><IonLabel>Vacantes</IonLabel>
                </IonTabButton>,
                <IonTabButton key="cc" tab="candidates" href="/company/candidates">
                  <IonIcon icon={peopleOutline} /><IonLabel>Candidatos</IonLabel>
                </IonTabButton>,
              ]
            : [
                <IonTabButton key="v" tab="vacancies" href="/candidate/vacancies">
                  <IonIcon icon={briefcaseOutline} /><IonLabel>Vacantes</IonLabel>
                </IonTabButton>,
                <IonTabButton key="a" tab="applications" href="/candidate/applications">
                  <IonIcon icon={documentTextOutline} /><IonLabel>Postulaciones</IonLabel>
                </IonTabButton>,
              ]}
          <IonTabButton tab="messages" href="/messages">
            <IonIcon icon={mailOutline} /><IonLabel>Mensajes</IonLabel>
          </IonTabButton>
          <IonTabButton tab="profile" href="/profile">
            <IonIcon icon={personOutline} /><IonLabel>Perfil</IonLabel>
          </IonTabButton>
        </IonTabBar>
      </IonTabs>
      </div>
    </>
  );
};

const App: React.FC = () => (
  <IonApp>
    <IonReactRouter>
      <AuthProvider>
        <Shell />
      </AuthProvider>
    </IonReactRouter>
  </IonApp>
);

export default App;
