#!/usr/bin/env node
// Real MCP SDK client: initialization -> tools/list -> tools/call -> transport close.
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import { mkdir, writeFile } from 'node:fs/promises';
const require = createRequire(new URL('../../mcp-server/package.json', import.meta.url));
const { Client } = require('@modelcontextprotocol/sdk/client/index.js');
const { StdioClientTransport } = require('@modelcontextprotocol/sdk/client/stdio.js');
const client = new Client({name: 'omr-baseline-verifier', version: '1.0.0'});
const transport = new StdioClientTransport({command: process.execPath,
  args: [fileURLToPath(new URL('./bridge_server.mjs', import.meta.url))],
  env: {...process.env}, stderr: 'inherit'});
const output = fileURLToPath(new URL('../../.artifacts/mcp/', import.meta.url));
try {
  await client.connect(transport);
  const catalog = await client.listTools();
  console.log('MCP tools:', catalog.tools.map(t => t.name).join(', '));
  const name = process.argv[2] ?? 'editor_get_status';
  const args = process.argv[3] ? JSON.parse(process.argv[3]) : {};
  const result = await client.callTool({name, arguments: args});
  if (result.isError) throw new Error(JSON.stringify(result.content));
  await mkdir(output, {recursive: true});
  for (const item of result.content) {
    if (item.type === 'image') {
      const path = `${output}/${name}.png`;
      await writeFile(path, Buffer.from(item.data, 'base64'));
      console.log('SCREENSHOT:', path);
    } else if (item.type === 'text') {
      await writeFile(`${output}/${name}.json`, item.text + '\n');
      console.log(item.text);
    }
  }
  console.log('MCP_CALL_PASS', name);
} finally { await client.close(); }
