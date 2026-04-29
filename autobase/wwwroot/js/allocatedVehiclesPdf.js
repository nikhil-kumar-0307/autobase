/* ── Shared html2pdf base options ── */
const _pdfBase = {
    image: { type: 'jpeg', quality: 0.98 },
    html2canvas: { scale: 2, useCORS: true, logging: false },
};

/* ── Helper: today string ── */
function todayStr() {
    return new Date().toLocaleDateString('en-IN', { day: 'numeric', month: 'long', year: 'numeric' });
}

/* ── Helper: now string ── */
function nowStr() {
    return new Date().toLocaleString();
}

/* ── Helper: initials from full name ── */
function getInitials(name) {
    return name.split(' ', 2).map(w => w[0] || '').join('').toUpperCase();
}

/* ── Helper: extract one card's data from DOM ── */
function extractCardData(card) {
    const get = (sel, fallback = '—') => {
        const el = card.querySelector(sel);
        return el ? el.textContent.trim() : fallback;
    };

    const isOverdue = card.classList.contains('overdue');
    const veh = get('.ac-veh-name');
    const reg = get('.ac-reg');
    const type = get('.ac-type-badge');
    const uname = get('.ac-uname');
    const uemp = get('.ac-uemp');
    const timeInfo = get('.ac-prog-label span:first-child');

    let start = '—', due = '—', dur = '—', purpose = '—';
    card.querySelectorAll('.ac-row').forEach(r => {
        const key = r.querySelector('.ac-key')?.textContent.trim();
        const val = r.querySelector('.ac-val')?.textContent.trim();
        if (key === 'Start') start = val;
        if (key === 'Due Return') due = val;
        if (key === 'Duration') dur = val;
        if (key === 'Purpose') purpose = val || '—';
    });

    return { veh, reg, type, isOverdue, start, due, dur, purpose, uname, uemp, timeInfo };
}

/* ── Helper: create a hidden container, render PDF, then remove ── */
function _renderPdf(html, opts) {
    const container = document.createElement('div');
    container.style.cssText = 'position:fixed;left:-9999px;top:0;z-index:-1;';
    container.innerHTML = html;
    document.body.appendChild(container);

    return html2pdf()
        .set(opts)
        .from(container.firstElementChild)
        .save()
        .then(() => document.body.removeChild(container));
}

/* ── Helper: parse a Date object into date & time strings ── */
function parseDatetime(dt) {
    const date = dt.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    const time = dt.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true }).toUpperCase();
    return { date, time };
}


/* ════════════════════════════════════════════════════════════
   downloadRequestPDF – NTPC-style Vehicle Requisition Form
════════════════════════════════════════════════════════════ */
function downloadRequestPDF(userName, empNo, mobile, designation, department,   // ← new params
    vehicleName, regNo, startRaw, endRaw, dur,
    purpose, adminNotes, requestedOn, status) {

    const startDt = new Date(startRaw);
    const endDt = new Date(endRaw);
    const { date: startDate, time: startTime } = parseDatetime(startDt);
    const { date: endDate, time: endTime } = parseDatetime(endDt);

    const safeName = vehicleName.replace(/\s+/g, '_');

    const html = `
    <div style="font-family: Arial, sans-serif; width: 210mm;
                background: #fff; box-sizing: border-box; padding: 18mm 14mm 14mm;
                position: relative; font-size: 12px; color: #000;">

      <!-- Top Header Row: Logo | Title | Location -->
      <table style="width:100%; border-collapse:collapse; margin-bottom:10px;">
        <tr>
          <td style="width:18%; vertical-align:middle;">
            <div style="border:2px solid #000; padding:6px 10px; display:inline-block;
                        text-align:center; line-height:1.3;">
              <div style="font-size:10px; color:#1a5ca8; letter-spacing:1px;">एनटीपीसी</div>
              <div style="font-size:13px; color:#1a5ca8; font-weight:900; letter-spacing:2px;">NTPC</div>
            </div>
          </td>
          <td style="text-align:center; vertical-align:middle;">
            <div style="font-size:17px; font-weight:900; letter-spacing:1px; text-decoration:underline;">
              VEHICLE REQUISITION FORM
            </div>
          </td>
          <td style="width:20%; vertical-align:middle; text-align:right;">
            <div style="border:2px solid #000; padding:6px 12px; display:inline-block; text-align:center;">
              <div style="font-size:11px; font-weight:700; font-style:italic;">कहलगांव</div>
              <div style="font-size:11px; font-weight:700; letter-spacing:1px;">KAHALGAON</div>
            </div>
          </td>
        </tr>
      </table>

      <!-- Main Form Table -->
      <table style="width:100%; border-collapse:collapse; border:1.5px solid #000;">

        <!-- Row 1: Name | Employee No -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; width:4%; font-weight:700; vertical-align:top;">1</td>
          <td style="border:1px solid #000; padding:6px 8px; width:46%; vertical-align:top;">
            Name : <strong>${userName}</strong>
          </td>
          <td style="border:1px solid #000; padding:6px 8px; width:4%; font-weight:700; vertical-align:top;">2</td>
          <td style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            Employee No.: <strong>${empNo}</strong>
          </td>
        </tr>

        <!-- Row 2: Designation | Department -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">3</td>
          <td style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            Designation:
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:160px;">
              &nbsp;<strong>${designation || '—'}</strong>&nbsp;
            </span>
          </td>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">4</td>
          <td style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            Department:
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:160px;">
              &nbsp;<strong>${department || '—'}</strong>&nbsp;
            </span>
          </td>
        </tr>

        <!-- Row 3: Type of Vehicle -->
        <tr>
          <td style="border:1px solid #000; padding:8px; font-weight:700; vertical-align:top;">5</td>
          <td colspan="3" style="border:1px solid #000; padding:8px; line-height:2.2; vertical-align:top;">
            Type of Vehicle Required :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:80px;">&nbsp;<strong>${vehicleName}</strong>&nbsp;</span>
            &nbsp; To report at (place)/Qtr. No.
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:80px;">&nbsp;</span>
            (on date)
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:90px;">&nbsp;<strong>${startDate}</strong>&nbsp;</span>
            <br/>
            (time) from
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:70px;">&nbsp;<strong>${startTime}</strong>&nbsp;</span>
            &nbsp;.....to.....&nbsp;
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:70px;">&nbsp;<strong>${endTime}</strong>&nbsp;</span>
            &nbsp; for
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:50px;">&nbsp;<strong>${dur}</strong>&nbsp;</span>
            <br/>
            Personal / Official use :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:200px;">&nbsp;Official&nbsp;</span>
          </td>
        </tr>

        <!-- Row 4: Purpose -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">(**)</td>
          <td colspan="3" style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            Please mentioned the purpose of Requirement :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:300px;">
              &nbsp;<strong>${purpose || '—'}</strong>&nbsp;
            </span>
          </td>
        </tr>

        <!-- Row 5: Signatures -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">6</td>
          <td style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            <div style="font-size:11px; font-weight:700;">Signature of Indentor</div>
            <div style="height:55px;"></div>
          </td>
          <td style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            <div style="font-size:11px; font-weight:700;">Signature of H.O.D</div>
            <div style="height:55px;"></div>
          </td>
          <td style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            <div style="font-size:11px; font-weight:700;">Signature of Approving Authority<br/>(Out station Journey)</div>
            <div style="height:40px;"></div>
          </td>
        </tr>

        <!-- Row 6: Requisition received -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">7</td>
          <td colspan="3" style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            Requisition received at Auto-Base Date
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:200px;">&nbsp;</span>
          </td>
        </tr>

        <!-- Row 7: Time & Vehicle allotted -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">8</td>
          <td colspan="3" style="border:1px solid #000; padding:6px 8px; vertical-align:top;">
            Time
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:100px;">&nbsp;</span>
            &nbsp;&nbsp;&nbsp;
            Vehicle allotted / Not available :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:180px;">&nbsp;<strong>${vehicleName} — ${regNo}</strong>&nbsp;</span>
          </td>
        </tr>

        <!-- FOR USE IN AUTO-BASE header -->
        <tr>
          <td colspan="2" style="border:1px solid #000; padding:7px 8px;
                                  text-align:center; font-weight:900; font-size:12px;
                                  background:#f0f0f0; letter-spacing:.5px;">
            FOR USE IN AUTO-BASE
          </td>
          <td colspan="2" style="border:1px solid #000; padding:7px 8px;
                                  font-weight:700; font-size:12px; background:#f0f0f0;">
            INDENT No. <span style="border-bottom:1px dotted #000; display:inline-block; min-width:120px;">&nbsp;</span>
          </td>
        </tr>

        <!-- Driver Instructions -->
        <tr>
          <td style="border:1px solid #000; padding:6px 8px; font-weight:700; vertical-align:top;">(*)</td>
          <td colspan="3" style="border:1px solid #000; padding:8px 10px; line-height:2.2; vertical-align:top;">
            <strong>Instruction to the Driver:</strong> Return this slip to Auto-Base after journey:
            <br/>
            Name of the Driver :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:280px;">&nbsp;</span>
            <br/>
            Vehicle Number &nbsp;&nbsp;:
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:280px;">&nbsp;<strong>${regNo}</strong>&nbsp;</span>
            <br/>
            K.M. Reading (out) :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:120px;">&nbsp;</span>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            K.M. Reading (in) :
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:120px;">&nbsp;</span>
            <br/>
            You are requested to report to Shri
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:200px;">&nbsp;</span>
            journey
            <br/>
            From
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:80px;">&nbsp;<strong>${startDate}</strong>&nbsp;</span>
            &nbsp;.....to.....&nbsp;
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:80px;">&nbsp;<strong>${endDate}</strong>&nbsp;</span>
            at
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:80px;">&nbsp;</span>
            <br/>
            today, i.e. (date) time
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:100px;">&nbsp;</span>
            &nbsp;&nbsp;&nbsp;
            Period
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:60px;">&nbsp;<strong>${dur}</strong>&nbsp;</span>
            &nbsp; for
            <span style="border-bottom:1px dotted #000; display:inline-block; min-width:100px;">&nbsp;</span>
          </td>
        </tr>

      </table>

      <!-- Signature of In-Charge -->
      <div style="text-align:right; margin-top:30px; font-size:12px; font-weight:700;">
        Signature of In-Charge
      </div>

    </div>`;

    _renderPdf(html, {
        ..._pdfBase,
        margin: 0,
        filename: `VehicleRequisition_${safeName}_${userName.replace(/\s+/g, '_')}_${new Date().toISOString().slice(0, 10)}.pdf`,
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
    });
}