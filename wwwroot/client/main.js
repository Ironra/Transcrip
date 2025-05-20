const startBtn = document.getElementById('start');
const stopBtn  = document.getElementById('stop');
const logEl    = document.getElementById('log');
let currentText = '';
let ws, audioCtx, processor, input;

// Abre el WebSocket
function openSocket() {
  const protocol = location.protocol === 'https:' ? 'wss' : 'ws';
  //const socketUrl = `${protocol}://${location.hostname}:${location.port}/ws`;
 const socketUrl = `${protocol}://transcrip-1.onrender.com/ws`;
  
  
  ws = new WebSocket(socketUrl);
  ws.onmessage = e => {
    const msg = JSON.parse(e.data);
    // VoskPartial -> msg.partial, VoskResult -> msg.text
    if (msg.partial) {
    // muestra el texto final + parcial flotando
    logEl .textContent = `${currentText}${msg.partial}`;
  } else if (msg.text) {
    // agrega al texto final y lo muestra
    currentText += msg.text + ' ';
    logEl.textContent = currentText;
  }
  };
}

startBtn.onclick = async () => {
  openSocket();

  // 1) Inicializa AudioContext
  audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 16000 });
  const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
  input = audioCtx.createMediaStreamSource(stream);

  // 2) Crea un ScriptProcessorNode para capturar PCM
  processor = audioCtx.createScriptProcessor(4096, 1, 1);
  processor.onaudioprocess = e => {
    const floatData = e.inputBuffer.getChannelData(0);
    // convierte Float32 [-1..1] -> Int16
    const int16 = new Int16Array(floatData.length);
    for (let i = 0; i < floatData.length; i++) {
      let s = Math.max(-1, Math.min(1, floatData[i]));
      int16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
    }
    // envía al servidor
    if (ws.readyState === WebSocket.OPEN) {
      ws.send(int16.buffer);
    }
  };

  // 3) Conecta todo
  input.connect(processor);
  processor.connect(audioCtx.destination);

  startBtn.disabled = true;
  stopBtn.disabled  = false;
};

stopBtn.onclick = () => {
  // 1) Cierra audio
  processor.disconnect();
  input.disconnect();
  audioCtx.close();

  // 2) Cierra WebSocket (el servidor te mandará el resultado final)
  ws.close();

  startBtn.disabled = false;
  stopBtn.disabled  = true;
};