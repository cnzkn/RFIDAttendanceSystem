import {
	createHashHistory,
	createRouter,
	RouterProvider,
} from "@tanstack/react-router";
import { StrictMode } from "react";
import ReactDOM from "react-dom/client";

import * as TanStackQueryProvider from "./integrations/tanstack-query/root-provider.tsx";

// Import the generated route tree
import { routeTree } from "./routeTree.gen";
import "./styles.css";

const hashHistory = createHashHistory();
const TanStackQueryProviderContext = TanStackQueryProvider.getContext();

const router = createRouter({
	routeTree,
	history: hashHistory,
	context: {
		...TanStackQueryProviderContext,
	},
	defaultPreload: "intent",
	scrollRestoration: true,
	defaultStructuralSharing: true,
	defaultPreloadStaleTime: 0,
	defaultPendingComponent: () => (
		<div className="min-h-screen bg-yellow-50 flex items-center justify-center">
			<div className="text-4xl font-black animate-pulse font-mono tracking-widest">
				LOADING_DATA...
			</div>
		</div>
	),
	defaultErrorComponent: () => (
		<div className="min-h-screen bg-yellow-50 flex items-center justify-center p-8">
			<div className="border-4 border-black bg-red-500 text-white p-8 text-2xl font-black shadow-[8px_8px_0px_0px_rgba(0,0,0,1)]">
				SYSTEM ERROR: COULD NOT LOAD SESSION
			</div>
		</div>
	),
});

// Register the router instance for type safety
declare module "@tanstack/react-router" {
	interface Register {
		router: typeof router;
	}
}

// Render the app
const rootElement = document.getElementById("app");
if (rootElement && !rootElement.innerHTML) {
	const root = ReactDOM.createRoot(rootElement);
	root.render(
		<StrictMode>
			<TanStackQueryProvider.Provider {...TanStackQueryProviderContext}>
				<RouterProvider router={router} />
			</TanStackQueryProvider.Provider>
		</StrictMode>,
	);
}
