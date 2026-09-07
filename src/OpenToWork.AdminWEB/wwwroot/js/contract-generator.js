window.contractGenerator = {
    generate: function (data) {
        const now = new Date();
        const dateStr = now.toLocaleDateString('es-ES', { year: 'numeric', month: 'long', day: 'numeric' });

        const selectedPlan = data.selectedPlanName || 'N/A';
        const planPrice = data.selectedPlanPrice || 'N/A';
        const currency = data.selectedPlanCurrency || 'EUR';

        const html = `<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>Contrato de Prestación de Servicios - ${data.companyName}</title>
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body {
        font-family: 'Segoe UI', 'Inter', system-ui, -apple-system, sans-serif;
        color: #0B132B;
        background: #f0f2f5;
        padding: 2rem 1rem;
        line-height: 1.6;
    }
    .contract-page {
        max-width: 800px;
        margin: 0 auto;
        background: #fff;
        padding: 3rem 3.5rem;
        border-radius: 8px;
        box-shadow: 0 4px 24px rgba(0,0,0,0.08);
    }
    .contract-header {
        text-align: center;
        border-bottom: 3px solid #0066FF;
        padding-bottom: 1.5rem;
        margin-bottom: 2rem;
    }
    .contract-header h1 {
        font-size: 1.5rem;
        color: #0B132B;
        margin-bottom: 0.25rem;
    }
    .contract-header p {
        font-size: 0.85rem;
        color: #778DA9;
    }
    .contract-meta {
        display: flex;
        justify-content: space-between;
        font-size: 0.82rem;
        color: #3A506B;
        margin-bottom: 2rem;
        padding: 0.75rem 1rem;
        background: #F1F5F9;
        border-radius: 8px;
    }
    h2 {
        font-size: 1.05rem;
        color: #0066FF;
        margin: 1.75rem 0 0.75rem;
        font-weight: 700;
    }
    p, li {
        font-size: 0.88rem;
        color: #3A506B;
        margin-bottom: 0.5rem;
        text-align: justify;
    }
    ul {
        margin-left: 1.5rem;
        margin-bottom: 0.75rem;
    }
    .highlight-box {
        background: #F0F7FF;
        border: 1px solid #E8F1FF;
        border-radius: 8px;
        padding: 1rem 1.25rem;
        margin: 1rem 0;
    }
    .highlight-box strong { color: #0066FF; }
    .signatures {
        display: flex;
        justify-content: space-between;
        margin-top: 3rem;
        gap: 2rem;
    }
    .sig-block {
        flex: 1;
        text-align: center;
    }
    .sig-line {
        border-top: 1.5px solid #0B132B;
        margin-top: 3rem;
        padding-top: 0.5rem;
        font-size: 0.8rem;
        color: #778DA9;
    }
    .sig-name {
        font-size: 0.85rem;
        font-weight: 600;
        color: #0B132B;
        margin-bottom: 2.5rem;
    }
    .print-btn {
        position: fixed;
        bottom: 2rem;
        right: 2rem;
        background: #0066FF;
        color: #fff;
        border: none;
        padding: 0.75rem 1.5rem;
        border-radius: 10px;
        font-size: 0.9rem;
        font-weight: 600;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(0,102,255,0.3);
        transition: background 0.2s;
    }
    .print-btn:hover { background: #0052CC; }
    @media print {
        body { background: #fff; padding: 0; }
        .contract-page { box-shadow: none; border-radius: 0; padding: 2rem; }
        .print-btn { display: none; }
    }
</style>
</head>
<body>
<div class="contract-page">
    <div class="contract-header">
        <h1>CONTRATO DE PRESTACIÓN DE SERVICIOS</h1>
        <p>Trato Directo - Plataforma de Gestión de Talento</p>
    </div>

    <div class="contract-meta">
        <div><strong>Contrato N°:</strong> TD-${now.getFullYear()}-${String(now.getMonth()+1).padStart(2,'0')}-${data.contractRef}</div>
        <div><strong>Fecha:</strong> ${dateStr}</div>
    </div>

    <h2>CLÁUSULA PRIMERA - PARTES</h2>
    <p>De una parte, <strong>Trato Directo</strong> (en adelante, "el Proveedor"), con domicilio en España, dedicada a la prestación de servicios de plataforma digital de gestión de talento y reclutamiento.</p>
    <p>De otra parte, <strong>${data.companyName}</strong>${data.legalName && data.legalName !== data.companyName ? ' (' + data.legalName + ')' : ''}${data.taxId ? ', con NIF/CIF: ' + data.taxId : ''}, ${data.address ? 'con domicilio en ' + data.address + ',' : ''} ${data.city ? data.city + ',' : ''} ${data.country || ''} (en adelante, "el Cliente"), representada en este acto por ${data.contactName || 'su representante legal'}${data.contactPosition ? ', cargo: ' + data.contactPosition : ''}.</p>

    <h2>CLÁUSULA SEGUNDA - OBJETO</h2>
    <p>El Proveedor pone a disposición del Cliente el acceso a la plataforma digital Trato Directo, que incluye servicios de publicación de vacantes, gestión de candidatos, evaluación técnica, verificación documental y herramientas de reclutamiento, conforme al plan contratado.</p>

    <div class="highlight-box">
        <p><strong>Plan contratado:</strong> ${selectedPlan}</p>
        <p><strong>Precio:</strong> ${planPrice} ${currency} / mes (más impuestos aplicables)</p>
        <p><strong>Vigencia:</strong> 12 meses a partir de la fecha de firma, renovable automáticamente.</p>
    </div>

    <h2>CLÁUSULA TERCERA - OBLIGACIONES DEL PROVEEDOR</h2>
    <ul>
        <li>Mantener la plataforma operativa 24/7 con un uptime mínimo del 99.5%.</li>
        <li>Proveer soporte técnico según el nivel del plan contratado.</li>
        <li>Garantizar la confidencialidad y protección de datos conforme al RGPD (Reglamento UE 2016/679).</li>
        <li>Facilitar la formación inicial para el uso de la plataforma.</li>
        <li>Realizar copias de seguridad diarias de los datos del Cliente.</li>
    </ul>

    <h2>CLÁUSULA CUARTA - OBLIGACIONES DEL CLIENTE</h2>
    <ul>
        <li>Abonar puntualmente las cuotas mensuales estipuladas en el plan contratado.</li>
        <li>Utilizar la plataforma conforme a las condiciones de uso y la normativa vigente.</li>
        <li>Proporcionar información veraz y actualizada sobre las vacantes y procesos de selección.</li>
        <li>No compartir las credenciales de acceso con terceros no autorizados.</li>
        <li>Cumprimir la normativa de protección de datos en el tratamiento de la información de candidatos.</li>
    </ul>

    <h2>CLÁUSULA QUINTA - PRECIO Y FORMA DE PAGO</h2>
    <p>El Cliente abonará al Proveedor una cuota mensual de <strong>${planPrice} ${currency}</strong> por los servicios del plan ${selectedPlan}. El pago se realizará mediante domiciliación bancaria o transferencia dentro de los primeros 10 días de cada mes. El impago de dos mensualidades consecutivas dará derecho al Proveedor a suspender temporalmente el servicio.</p>

    <h2>CLÁUSULA SEXTA - DURACIÓN Y RESOLUCIÓN</h2>
    <p>El presente contrato tendrá una duración de 12 meses, renovándose automáticamente por períodos iguales salvo denuncia de cualquiera de las partes con un preaviso mínimo de 30 días naturales antes de la fecha de vencimiento.</p>
    <p>El contrato podrá resolverse anticipadamente por: (a) mutuo acuerdo, (b) incumplimiento grave de cualquiera de las cláusulas, (c) quiebra o insolvencia de alguna de las partes.</p>

    <h2>CLÁUSULA SÉPTIMA - CONFIDENCIALIDAD Y PROTECCIÓN DE DATOS</h2>
    <p>Ambas partes se comprometen a mantener la confidencialidad de toda la información a la que tengan acceso durante la vigencia del contrato. El tratamiento de datos personales se realizará conforme al RGPD y la LOPDGDD (Ley Orgánica 3/2018). El Proveedor actuará como encargado del tratamiento y el Cliente como responsable del mismo.</p>

    <h2>CLÁUSULA OCTAVA - PROPIEDAD INTELECTUAL</h2>
    <p>La plataforma Trato Directo, incluyendo su código, diseño, marcas y contenidos, es propiedad exclusiva del Proveedor. El Cliente recibe una licencia de uso no exclusiva e intransferible durante la vigencia del contrato.</p>

    <h2>CLÁUSULA NOVENA - LIMITACIÓN DE RESPONSABILIDAD</h2>
    <p>El Proveedor no será responsable de los daños indirectos, lucro cesante o pérdida de oportunidades derivados del uso de la plataforma. La responsabilidad total del Proveedor quedará limitada al importe abonado por el Cliente en los últimos 3 meses de facturación.</p>

    <h2>CLÁUSULA DÉCIMA - LEGISLACIÓN APLICABLE Y JURISDICCIÓN</h2>
    <p>El presente contrato se regirá por la legislación española. Para la resolución de cualesquiera controversias, las partes se someten a los juzgados y tribunales de la ciudad donde tenga su sede el Proveedor, con renuncia expresa a cualquier otro fuero que pudiera corresponderles.</p>

    <h2>CLÁUSULA UNDÉCIMA - ACEPTACIÓN</h2>
    <p>Ambas partes reconocen haber leído el presente contrato y aceptan íntegramente su contenido, firmando en prueba de conformidad en todas sus cláusulas.</p>

    <div class="signatures">
        <div class="sig-block">
            <p class="sig-name">Trato Directo<br /><small style="font-weight:400;color:#778DA9;">El Proveedor</small></p>
            <div class="sig-line">Firma y sello</div>
        </div>
        <div class="sig-block">
            <p class="sig-name">${data.companyName}<br /><small style="font-weight:400;color:#778DA9;">El Cliente</small></p>
            <div class="sig-line">Firma y sello</div>
        </div>
    </div>
</div>

<button class="print-btn" onclick="window.print()">
    🖨️ Imprimir / Guardar PDF
</button>
</body>
</html>`;

        const w = window.open('', '_blank');
        if (w) {
            w.document.write(html);
            w.document.close();
        } else {
            alert('Permita las ventanas emergentes para generar el contrato.');
        }
    }
};
