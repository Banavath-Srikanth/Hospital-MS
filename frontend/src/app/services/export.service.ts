import { Injectable } from '@angular/core';

export interface PdfColumn {
  header: string;
  key: string;
  width?: number;
}

@Injectable({ providedIn: 'root' })
export class ExportService {

  /**
   * Export data rows to a styled PDF using jsPDF + autoTable.
   */
  async exportToPdf(
    title: string,
    subtitle: string,
    columns: PdfColumn[],
    rows: Record<string, any>[],
    filename: string
  ): Promise<void> {
    const { default: jsPDF } = await import('jspdf');
    const autoTable = (await import('jspdf-autotable')).default;

    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });

    // ── Header band ────────────────────────────────────────────
    doc.setFillColor(30, 45, 86);       // Kendo dark navy
    doc.rect(0, 0, 297, 22, 'F');

    doc.setTextColor(200, 214, 240);
    doc.setFontSize(14);
    doc.setFont('helvetica', 'bold');
    doc.text(title, 14, 10);

    doc.setFontSize(8);
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(150, 180, 220);
    doc.text(subtitle, 14, 17);

    // Date stamp top-right
    const now = new Date().toLocaleString('en-IN', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
    doc.setFontSize(7.5);
    doc.setTextColor(170, 195, 230);
    doc.text(`Generated: ${now}`, 283, 17, { align: 'right' });

    // ── Auto table ─────────────────────────────────────────────
    const head = [columns.map(c => c.header)];
    const body = rows.map(r => columns.map(c => r[c.key] ?? ''));

    autoTable(doc, {
      head,
      body,
      startY: 26,
      margin: { left: 14, right: 14 },
      styles: {
        fontSize: 8.5,
        cellPadding: { top: 3, bottom: 3, left: 4, right: 4 },
        overflow: 'linebreak',
        lineColor: [199, 211, 224],
        lineWidth: 0.2,
      },
      headStyles: {
        fillColor: [44, 62, 107],
        textColor: [200, 214, 240],
        fontStyle: 'bold',
        fontSize: 8,
        halign: 'left',
      },
      alternateRowStyles: {
        fillColor: [245, 248, 252],
      },
      bodyStyles: {
        textColor: [51, 65, 85],
      },
      columnStyles: this.buildColumnStyles(columns),
      didDrawPage: (data: any) => {
        // Footer on each page
        const pageCount = (doc as any).internal.getNumberOfPages();
        doc.setFontSize(7);
        doc.setTextColor(150, 160, 175);
        doc.text(
          `Page ${data.pageNumber} of ${pageCount}  |  Hospital Management System`,
          148.5, 207,
          { align: 'center' }
        );
      }
    });

    doc.save(filename);
  }

  /**
   * Open a styled print window with a table of the given data.
   */
  printTable(
    title: string,
    subtitle: string,
    columns: PdfColumn[],
    rows: Record<string, any>[]
  ): void {
    const now = new Date().toLocaleString('en-IN', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });

    const thead = `<tr>${columns.map(c => `<th>${c.header}</th>`).join('')}</tr>`;
    const tbody = rows.map((r, i) =>
      `<tr class="${i % 2 === 1 ? 'alt' : ''}">${columns.map(c => `<td>${r[c.key] ?? ''}</td>`).join('')}</tr>`
    ).join('');

    const html = `<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8"/>
  <title>${title}</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; color: #1e293b; background: #fff; }

    .print-header {
      background: linear-gradient(135deg, #1e2d56, #2c3e6b);
      color: #c8d6f0;
      padding: 14px 20px 10px;
      margin-bottom: 16px;
    }
    .print-header h1 { font-size: 18px; font-weight: 700; color: #fff; margin-bottom: 3px; }
    .print-header p  { font-size: 10px; color: #aac4e4; }
    .print-header .meta {
      float: right; text-align: right; font-size: 9px;
      color: #9ab8d8; margin-top: -30px;
    }

    table {
      width: 100%; border-collapse: collapse;
      margin: 0 20px; width: calc(100% - 40px);
    }
    thead tr { background: #1e2d56; }
    th {
      padding: 8px 10px; text-align: left;
      font-size: 9.5px; font-weight: 700;
      text-transform: uppercase; letter-spacing: 0.05em;
      color: #c8d6f0; border-right: 1px solid rgba(255,255,255,0.08);
    }
    th:last-child { border-right: none; }
    td {
      padding: 7px 10px; font-size: 10.5px;
      border-bottom: 1px solid #dde5ef;
      border-right: 1px solid #e8edf4;
      color: #334155; vertical-align: middle;
    }
    td:last-child { border-right: none; }
    tr.alt td { background: #f5f8fc; }

    .print-footer {
      margin-top: 14px; padding: 8px 20px;
      font-size: 9px; color: #94a3b8;
      border-top: 1px solid #e2e8f0;
      display: flex; justify-content: space-between;
    }

    @media print {
      @page { margin: 10mm; size: landscape; }
      body  { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
      thead { display: table-header-group; }
    }
  </style>
</head>
<body>
  <div class="print-header">
    <h1>${title}</h1>
    <p>${subtitle}</p>
    <div class="meta">Generated: ${now}<br/>${rows.length} record(s)</div>
  </div>
  <table>
    <thead>${thead}</thead>
    <tbody>${tbody}</tbody>
  </table>
  <div class="print-footer">
    <span>Hospital Management System</span>
    <span>${now}</span>
  </div>
  <script>window.onload = () => { window.print(); window.onafterprint = () => window.close(); };<\/script>
</body>
</html>`;

    const win = window.open('', '_blank', 'width=1100,height=800');
    if (win) {
      win.document.write(html);
      win.document.close();
    }
  }

  private buildColumnStyles(columns: PdfColumn[]): Record<number, any> {
    const styles: Record<number, any> = {};
    columns.forEach((c, i) => {
      if (c.width) styles[i] = { cellWidth: c.width };
    });
    return styles;
  }
}
