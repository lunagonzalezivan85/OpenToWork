import { useState } from 'react';
import {
  IonCard, IonCardContent, IonCardHeader, IonCardSubtitle, IonCardTitle,
  IonContent, IonPage, IonSpinner, useIonViewWillEnter,
} from '@ionic/react';
import PageHeader from '../components/PageHeader';
import { api } from '../services/api';

interface NewsItem {
  slug: string;
  title: string;
  summary?: string;
  publishedAt?: string;
}

export default function News() {
  const [items, setItems] = useState<NewsItem[] | null>(null);

  useIonViewWillEnter(() => {
    void api.get<NewsItem[]>('/api/news')
      .then(setItems)
      .catch(() => setItems([]));
  });

  return (
    <IonPage>
      <PageHeader title="Noticias" />
      <IonContent className="ion-padding">
        {items === null && <div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div>}
        {items?.length === 0 &&
          <p style={{ textAlign: 'center', color: 'var(--ion-color-tertiary)', padding: 40 }}>Sin noticias.</p>}
        {items?.map(n => (
          <IonCard key={n.slug}>
            <IonCardHeader>
              {n.publishedAt && <IonCardSubtitle>{new Date(n.publishedAt).toLocaleDateString('es-ES')}</IonCardSubtitle>}
              <IonCardTitle>{n.title}</IonCardTitle>
            </IonCardHeader>
            {n.summary && <IonCardContent>{n.summary}</IonCardContent>}
          </IonCard>
        ))}
      </IonContent>
    </IonPage>
  );
}
