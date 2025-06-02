const startBtn = document.getElementById('start');
const stopBtn  = document.getElementById('stop');
const logEl    = document.getElementById('log');
let textoFinal = '';

let ws, audioCtx, processor, input;

function openSocket() {
  const protocol = location.protocol === 'https:' ? 'wss' : 'ws';
  const socketUrl = protocol + '://' + location.host + '/ws';

  ws = new WebSocket(socketUrl);
  ws.onmessage = e => {
    console.log('WebSocket abierto');
    const msg = JSON.parse(e.data);
    if (msg.partial) {
      logEl.textContent = `${textoFinal}${msg.partial}`;
    } else if (msg.text) {
      textoFinal += msg.text + ' ';
      logEl.textContent = textoFinal;
    }
  };
}
startBtn.onclick = async () => {
  openSocket(); 
  audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 16000 });
  const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
  input = audioCtx.createMediaStreamSource(stream);
  processor = audioCtx.createScriptProcessor(4096, 1, 1); 
  processor.onaudioprocess = e => {
    console.log('onaudioprocess fired');
    const floatData = e.inputBuffer.getChannelData(0);
    const int16 = new Int16Array(floatData.length);
    for (let i = 0; i < floatData.length; i++) {
      let s = Math.max(-1, Math.min(1, floatData[i]));
      int16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
    }
    if (ws.readyState === WebSocket.OPEN) {
      console.log('Enviando audio:', int16.byteLength, 'bytes');
      ws.send(int16.buffer);
    }
  };
  input.connect(processor);
  processor.connect(audioCtx.destination);

  startBtn.disabled = true;
  stopBtn.disabled  = false;
};
stopBtn.onclick = () => {
  processor.disconnect();
  input.disconnect();
  audioCtx.close();
  ws.close();
  startBtn.disabled = false;
  stopBtn.disabled  = true;
};
