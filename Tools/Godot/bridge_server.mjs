#!/usr/bin/env node
// Thin, project-local MCP adapter; reuses the repository's pinned MCP SDK.
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import { tools, validateCall, requestBridge } from './bridge.mjs';

const require = createRequire(new URL('../../mcp-server/package.json', import.meta.url));
const { Server } = require('@modelcontextprotocol/sdk/server/index.js');
const { StdioServerTransport } = require('@modelcontextprotocol/sdk/server/stdio.js');
const { ListToolsRequestSchema, CallToolRequestSchema } = require('@modelcontextprotocol/sdk/types.js');
const allowControl = process.env.OMR_MCP_ALLOW_CONTROL === '1';
const projectPath = fileURLToPath(new URL('../../OpenMakaiRanchGame/', import.meta.url));
const normalize = value => String(value).replaceAll('\\', '/').replace(/\/$/, '').toLowerCase();
const server = new Server({name: 'openmakairanch-godot-bridge', version: '1.0.0'}, {capabilities: {tools: {}}});

server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools: tools.filter(t => allowControl || !t.control).map(t => ({
    name: t.name.replaceAll('.', '_'), description: t.description,
    inputSchema: {type: 'object', additionalProperties: false,
      properties: t.scene ? {scene_path: {type: 'string'}} : {}, required: t.scene ? ['scene_path'] : []},
  })),
}));
server.setRequestHandler(CallToolRequestSchema, async request => {
  try {
    const name = tools.find(t => t.name.replaceAll('.', '_') === request.params.name)?.name;
    const args = request.params.arguments ?? {};
    const tool = validateCall(name, args, allowControl);
    // Read back identity before every action; never control another open project.
    const status = await requestBridge(9500, 'editor.get_status', {}, 10000, projectPath);
    if (normalize(status.projectPath) !== normalize(projectPath)) throw new Error('Editor belongs to a different project');
    const value = name === 'editor.get_status' ? status : await requestBridge(tool.port, name, args, 10000, projectPath);
    if (tool.scene) {
      const after = await requestBridge(9500, 'editor.get_status', {}, 10000, projectPath);
      if (after.currentScene !== args.scene_path) throw new Error('Editor did not switch to the requested scene');
    }
    if (name === 'runtime.screenshot') {
      return {content: [{type: 'image', mimeType: 'image/png', data: value.base64}]};
    }
    return {content: [{type: 'text', text: JSON.stringify(value)}]};
  } catch (error) {
    return {isError: true, content: [{type: 'text', text: error.message}]};
  }
});

await server.connect(new StdioServerTransport());
