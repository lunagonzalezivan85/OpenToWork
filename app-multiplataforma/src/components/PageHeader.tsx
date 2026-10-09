import type { ReactNode } from 'react';
import { IonButtons, IonHeader, IonMenuButton, IonTitle, IonToolbar } from '@ionic/react';

/** Cabecera comun: titulo + boton de hamburguesa que abre AppMenu.
 *  `children` permite barras extra (p.ej. buscador) bajo el titulo. */
export default function PageHeader({ title, children }: { title: string; children?: ReactNode }) {
  return (
    <IonHeader>
      <IonToolbar>
        <IonButtons slot="start"><IonMenuButton autoHide={false} /></IonButtons>
        <IonTitle>{title}</IonTitle>
      </IonToolbar>
      {children}
    </IonHeader>
  );
}
