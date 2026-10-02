// Verification harness: loads the AlpeGram Pi extension with jiti (the same
// loader Pi uses), captures the registered tools via a mock ExtensionAPI, and
// executes each mem_* tool against the running daemon.
import { createJiti } from "jiti";
import { fileURLToPath } from "node:url";

const jiti = createJiti(import.meta.url, { moduleCache: false });
const factory = await jiti.import("C:/Users/alexlocal/projects/AlpeGram/pi-extension/index.ts", { default: true });

const tools = new Map();
const mockPi = {
	registerTool: (t) => tools.set(t.name, t),
	registerCommand: () => {},
	on: () => () => {},
};

await factory(mockPi);
console.log(`Loaded extension. Registered ${tools.size} tools: ${[...tools.keys()].join(", ")}`);

// Minimal fake ctx with a cwd.
const ctx = { cwd: "C:/Users/alexlocal/projects/AlpefePI" };

async function run(name, params) {
	const t = tools.get(name);
	if (!t) throw new Error(`tool ${name} not registered`);
	const res = await t.execute("test-id", params, undefined, undefined, ctx);
	const text = res.content?.map((c) => c.text).join("\n") ?? "";
	console.log(`\n### ${name} ${JSON.stringify(params)}\n${text}`);
	return res;
}

// 1. current project
await run("mem_current_project", {});
// 2. save
await run("mem_save", {
	title: "Verification observation",
	content: "AlpeGram Pi extension verified end-to-end via jiti harness",
	what: "Ran verification",
	why: "Confirm native integration",
	where: "pi-extension/index.ts",
	learned: "jiti loads the extension and tools execute against the daemon",
	topicKey: "verification/integration",
	type: "discovery",
});
// 3. search
await run("mem_search", { query: "verification" });
// 4. context
await run("mem_context", { limit: 5 });
// 5. topics
await run("mem_suggest_topic_key", {});
// 6. session summary
await run("mem_session_summary", {
	summary: "Verified AlpeGram Pi extension",
	goal: "Confirm native tooling",
	nextSteps: "Commit and push",
	sessionId: "verify-sess",
});

console.log("\nALL TOOLS EXECUTED OK");
