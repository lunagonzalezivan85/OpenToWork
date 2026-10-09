import {
  IonContent, IonHeader, IonItem, IonLabel, IonList, IonMenu,
  IonTitle, IonToolbar, IonIcon, useIonRouter,
} from '@ionic/react';
import {
  briefcaseOutline, documentTextOutline, flagOutline, homeOutline,
  logOutOutline, mailOutline, newspaperOutline, peopleOutline,
  personOutline, ribbonOutline,
} from 'ionicons/icons';
import { useAuth } from '../auth/AuthContext';

interface Entry { label: string; path: string; icon: string }

/** Menu lateral completo por rol - el mismo set de secciones que el portal web. */
export default function AppMenu() {
  const { user, logout } = useAuth();
  const router = useIonRouter();
  if (!user) return null;

  const isCompany = user.primaryRole === 1;

  const entries: Entry[] = isCompany
    ? [
        { label: 'Panel', path: '/company/dashboard', icon: homeOutline },
        { label: 'Mis vacantes', path: '/company/vacancies', icon: briefcaseOutline },
        { label: 'Buscar candidatos', path: '/company/candidates', icon: peopleOutline },
        { label: 'Candidatos entregados', path: '/company/deliveries', icon: ribbonOutline },
        { label: 'Mensajes', path: '/messages', icon: mailOutline },
        { label: 'Noticias', path: '/news', icon: newspaperOutline },
        { label: 'Perfil', path: '/profile', icon: personOutline },
      ]
    : [
        { label: 'Panel', path: '/candidate/dashboard', icon: homeOutline },
        { label: 'Vacantes', path: '/candidate/vacancies', icon: briefcaseOutline },
        { label: 'Mis postulaciones', path: '/candidate/applications', icon: documentTextOutline },
        { label: 'Mi proceso', path: '/candidate/process', icon: flagOutline },
        { label: 'Mensajes', path: '/messages', icon: mailOutline },
        { label: 'Noticias', path: '/news', icon: newspaperOutline },
        { label: 'Perfil', path: '/profile', icon: personOutline },
      ];

  return (
    <IonMenu contentId="main" side="start" type="overlay">
      <IonHeader>
        <IonToolbar color="primary"><IonTitle>Trato Directo</IonTitle></IonToolbar>
      </IonHeader>
      <IonContent>
        <IonList>
          {entries.map(e => (
            <IonItem key={e.path} button detail={false}
              onClick={() => router.push(e.path, 'root')}>
              <IonIcon slot="start" icon={e.icon} />
              <IonLabel>{e.label}</IonLabel>
            </IonItem>
          ))}
          <IonItem button detail={false} lines="none" onClick={() => void logout()}>
            <IonIcon slot="start" icon={logOutOutline} color="danger" />
            <IonLabel color="danger">Cerrar sesión</IonLabel>
          </IonItem>
        </IonList>
      </IonContent>
    </IonMenu>
  );
}
