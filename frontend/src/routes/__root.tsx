import { TanStackDevtools } from "@tanstack/react-devtools";
import type { QueryClient } from "@tanstack/react-query";
import {
	createRootRouteWithContext,
	Outlet,
	redirect,
	useLocation,
} from "@tanstack/react-router";
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools";
import { Navbar } from "@/components/Navbar";
import TanStackQueryDevtools from "../integrations/tanstack-query/devtools";

interface MyRouterContext {
	queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<MyRouterContext>()({
	// This runs BEFORE the component renders
	beforeLoad: ({ location }) => {
		// 1. Check strict exclusion first!
		if (location.pathname === "/login") {
			return; // Don't interfere if they are going to login
		}

		// 2. Check local storage
		const token = localStorage.getItem("token");

		// 3. If no token, throw a redirect
		if (!token) {
			throw redirect({
				to: "/login",
			});
		}
	},
	component: () => {
		const location = useLocation();
		const isLoginPage = location.pathname === "/login";

		return (
			<>
				{!isLoginPage && <Navbar />}
				<Outlet />
				<TanStackDevtools
					config={{
						position: "bottom-right",
					}}
					plugins={[
						{
							name: "Tanstack Router",
							render: <TanStackRouterDevtoolsPanel />,
						},
						TanStackQueryDevtools,
					]}
				/>
			</>
		);
	},
});
