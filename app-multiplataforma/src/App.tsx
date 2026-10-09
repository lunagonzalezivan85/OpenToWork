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
import Login from './pages/Login';
import Vacancies from './pages/candidate/Vacancies';
import Applications from './pages/candidate/Applications';
import MyVacancies from './pages/company/MyVacancies';
import CandidateSearch from './pages/company/CandidateSearch';
import Messages from './pages/Messages';
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

  return (
    <IonTabs>
      <IonRouterOutlet>
        {/* Candidato */}
        <Route path="/candidate/vacancies" element={<Vacancies />} />
        <Route path="/candidate/applications" element={<Applications />} />
        {/* Empresa */}
        <Route path="/company/vacancies" element={<MyVacancies />} />
        <Route path="/company/candidates" element={<CandidateSearch />} />
        {/* Compartidas */}
        <Route path="/messages" element={<Messages />} />
        <Route path="/profile" element={<Profile />} />
        <Route path="/" element={
          <Navigate to={isCompany ? '/company/vacancies' : '/candidate/vacancies'} replace />
        } />
      </IonRouterOutlet>

      <IonTabBar slot="bottom">
        {isCompany ? (
          <>
            <IonTabButton tab="company-vacancies" href="/company/vacancies">
              <IonIcon icon={briefcaseOutline} /><IonLabel>Vacantes</IonLabel>
            </IonTabButton>
            <IonTabButton tab="company-candidates" href="/company/candidates">
              <IonIcon icon={peopleOutline} /><IonLabel>Candidatos</IonLabel>
            </IonTabButton>
          </>
        ) : (
          <>
            <IonTabButton tab="candidate-vacancies" href="/candidate/vacancies">
              <IonIcon icon={briefcaseOutline} /><IonLabel>Vacantes</IonLabel>
            </IonTabButton>
            <IonTabButton tab="candidate-applications" href="/candidate/applications">
              <IonIcon icon={documentTextOutline} /><IonLabel>Postulaciones</IonLabel>
            </IonTabButton>
          </>
        )}
        <IonTabButton tab="messages" href="/messages">
          <IonIcon icon={mailOutline} /><IonLabel>Mensajes</IonLabel>
        </IonTabButton>
        <IonTabButton tab="profile" href="/profile">
          <IonIcon icon={personOutline} /><IonLabel>Perfil</IonLabel>
        </IonTabButton>
      </IonTabBar>
    </IonTabs>
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
