import { useState } from 'react';
import {
  IonAvatar, IonBadge, IonContent, IonHeader, IonItem, IonLabel, IonList,
  IonNote, IonPage, IonRefresher, IonRefresherContent, IonSpinner, IonTitle,
  IonToolbar, useIonViewWillEnter,
} from '@ionic/react';
import { useIonRouter } from '@ionic/react';
import PageHeader from '../components/PageHeader';
import { api, ApiError } from '../services/api';
import type { Conversation } from '../types';
import { useAuth } from '../auth/AuthContext';

export default function Messages() {
  const { logout } = useAuth();
  const router = useIonRouter();
  const [items, setItems] = useState<Conversation[]>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      setItems(await api.get<Conversation[]>('/api/messages/conversations'));
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) void logout();
    } finally {
      setLoading(false);
    }
  };

  useIonViewWillEnter(() => { void load(); });

  return (
    <IonPage>
      <PageHeader title="Mensajes" />
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={async e => { await load(); e.detail.complete(); }}>
          <IonRefresherContent />
        </IonRefresher>
        {loading && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {!loading && items.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Sin conversaciones.</p>}
        <IonList inset>
          {items.map(c => (
            <IonItem key={c.id} button detail onClick={() => router.push(`/messages/${c.id}`, 'forward')}>
              <IonAvatar slot="start">
                <div style={{ width: '100%', height: '100%', display: 'grid', placeItems: 'center', background: 'var(--ion-color-primary)', color: '#fff', fontWeight: 700 }}>
                  {(c.participantAvatar || c.participantName.slice(0, 2)).toUpperCase()}
                </div>
              </IonAvatar>
              <IonLabel>
                <h2>{c.participantName}</h2>
                {c.lastMessage && <p>{c.lastMessage}</p>}
              </IonLabel>
              {c.unreadCount > 0 && <IonBadge slot="end" color="primary">{c.unreadCount}</IonBadge>}
            </IonItem>
          ))}
        </IonList>
      </IonContent>
    </IonPage>
  );
}
