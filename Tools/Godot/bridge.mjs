import net from 'node:net';
import { randomUUID } from 'node:crypto';
import { statSync } from 'node:fs';

const normalizeProject = value => String(value).replaceAll('\\', '/').replace(/\/$/, '').toLowerCase();

// This project's addon uses dotted tool names. It is not Coding-Solo's server.
export const tools = [
  { name: 'editor.get_status', description: 'Read the live editor version and exact project path.', port: 9500 },
  { name: 'editor.get_scene_tree', description: 'Read the currently edited scene tree.', port: 9500 },
  { name: 'runtime.get_status', description: 'Read the running scene, paused state and FPS.', port: 9501 },
  { name: 'runtime.get_tree', description: 'Read the live runtime node tree (addon depth limit: 4).', port: 9501 },
  { name: 'runtime.get_perf', description: 'Read FPS and static memory; not a GPU profiler.', port: 9501 },
  { name: 'runtime.screenshot', description: 'Capture the running viewport as PNG; requires a graphical runtime.', port: 9501 },
  { name: 'editor.open_scene', description: 'Open an existing scene in this project (opt-in editor control).', port: 9500, control: true, scene: true },
  { name: 'editor.run_project', description: 'Start the configured main scene (opt-in editor control).', port: 9500, control: true },
  { name: 'editor.stop_project', description: 'Stop the editor-managed game (opt-in editor control).', port: 9500, control: true },
];

export function validateCall(name, args, allowControl = false) {
  const tool = tools.find(t => t.name === name);
  if (!tool) throw new Error(`Unsupported tool: ${name}`);
  if (tool.control && !allowControl) throw new Error('Editor control requires OMR_MCP_ALLOW_CONTROL=1');
  if (!args || typeof args !== 'object' || Array.isArray(args)) throw new Error('Arguments must be an object');
  const allowed = tool.scene ? ['scene_path'] : [];
  if (Object.keys(args).some(key => !allowed.includes(key))) throw new Error('Unexpected argument');
  if (tool.scene && (typeof args.scene_path !== 'string' ||
      !/^res:\/\/scenes\/[a-zA-Z0-9_/-]+\.tscn$/.test(args.scene_path) || args.scene_path.includes('..'))) {
    throw new Error('scene_path must be a project scene under res://scenes/');
  }
  if (tool.scene) {
    const file = new URL('../../OpenMakaiRanchGame/' + args.scene_path.slice(6), import.meta.url);
    if (!statSync(file, {throwIfNoEntry: false})?.isFile()) throw new Error('Scene does not exist');
  }
  return tool;
}

export function requestBridge(port, tool, params, timeoutMs = 10000, expectedProjectPath) {
  return new Promise((resolve, reject) => {
    const id = randomUUID();
    const socket = net.createConnection({host: '127.0.0.1', port});
    let buffer = Buffer.alloc(0);
    const finish = (error, value) => {
      clearTimeout(timer);
      socket.destroy();
      if (error) reject(error); else resolve(value);
    };
    const timer = setTimeout(() => finish(new Error(`Bridge ${port} timed out after ${timeoutMs}ms`)), timeoutMs);
    socket.once('error', error => finish(error));
    socket.once('connect', () => socket.write(JSON.stringify({id, tool, params, expected_project_path: expectedProjectPath}) + '\n'));
    socket.on('data', chunk => {
      buffer = Buffer.concat([buffer, chunk]);
      if (buffer.length > 16 * 1024 * 1024) return finish(new Error('Bridge response exceeds 16 MiB'));
      const end = buffer.indexOf(10);
      if (end < 0) return;
      try {
        const response = JSON.parse(buffer.subarray(0, end).toString('utf8'));
        if (response.id !== id) throw new Error('Mismatched bridge response ID');
        if (expectedProjectPath && normalizeProject(response.projectPath) !== normalizeProject(expectedProjectPath)) {
          throw new Error('Bridge project identity is missing or mismatched');
        }
        if (response.ok !== true) throw new Error(response.error?.message ?? 'Bridge operation failed');
        finish(null, response.result);
      } catch (error) { finish(error); }
    });
    socket.once('end', () => {
      if (buffer.indexOf(10) < 0) finish(new Error('Bridge disconnected before a complete response'));
    });
  });
}
