import test from 'node:test';
import assert from 'node:assert/strict';
import net from 'node:net';
import { requestBridge, validateCall } from './bridge.mjs';

test('only known read operations are accepted by default', () => {
  assert.equal(validateCall('editor.get_status', {}).port, 9500);
  assert.equal(validateCall('runtime.get_tree', {}).port, 9501);
  assert.throws(() => validateCall('runtime.set_property', {}), /Unsupported/);
  assert.throws(() => validateCall('editor.run_project', {}), /OMR_MCP_ALLOW_CONTROL/);
  assert.throws(() => validateCall('editor.get_status', {evil: true}), /Unexpected/);
});

test('scene opening rejects missing files, not only invalid path syntax', () => {
  assert.throws(() => validateCall('editor.open_scene', {scene_path: 'res://scenes/__review_nonexistent_scene__.tscn'}, true), /does not exist/);
  assert.throws(() => validateCall('editor.open_scene', {scene_path: 'res://scenes/../outside.tscn'}, true), /scene_path/);
  assert.equal(validateCall('editor.open_scene', {scene_path: 'res://scenes/Bootstrap.tscn'}, true).name, 'editor.open_scene');
});

test('framing survives chunked replies, preserving request IDs', async () => {
  const server = net.createServer(socket => {
    socket.once('data', data => {
      const request = JSON.parse(data.toString());
      const reply = JSON.stringify({id: request.id, type: 'response', ok: true, result: {projectPath: 'fixture'}}) + '\n';
      socket.write(reply.slice(0, 15));
      setTimeout(() => socket.end(reply.slice(15)), 5);
    });
  });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  try {
    assert.deepEqual(await requestBridge(server.address().port, 'editor.get_status', {}, 1000), {projectPath: 'fixture'});
  } finally { server.close(); }
});

test('silence times out rather than hanging MCP', async () => {
  const sockets = [];
  const server = net.createServer(socket => sockets.push(socket));
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  try {
    await assert.rejects(requestBridge(server.address().port, 'runtime.get_tree', {}, 30), /timed out/);
  } finally { sockets.forEach(s => s.destroy()); server.close(); }
});

test('mismatched reply IDs fail closed', async () => {
  const server = net.createServer(socket => socket.once('data', () => socket.end('{"id":"wrong","ok":true}\n')));
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  try { await assert.rejects(requestBridge(server.address().port, 'editor.get_status', {}, 1000), /Mismatched/); }
  finally { server.close(); }
});

test('each response must match the exact expected project', async () => {
  for (const projectPath of [undefined, 'E:/other-project/', 'E:/expected-project/']) {
    let request;
    const server = net.createServer(socket => socket.once('data', data => {
      request = JSON.parse(data.toString());
      socket.end(JSON.stringify({id: request.id, ok: true, projectPath, result: {verified: true}}) + '\n');
    }));
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    try {
      const result = requestBridge(server.address().port, 'runtime.get_status', {}, 1000, 'E:/expected-project/');
      if (projectPath === 'E:/expected-project/') assert.deepEqual(await result, {verified: true});
      else await assert.rejects(result, /project identity/);
      assert.equal(request.expected_project_path, 'E:/expected-project/');
    } finally { server.close(); }
  }
});
