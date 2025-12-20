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
import { fetchCurrentUser } from "@/lib/api";
import TanStackQueryDevtools from "../integrations/tanstack-query/devtools";

interface MyRouterContext {
	queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<MyRouterContext>()({
	beforeLoad: async ({ location, context }) => {
		if (location.pathname === "/login") {
			return;
		}

		try {
			await context.queryClient.fetchQuery({
				queryKey: ["currentUser"],
				queryFn: fetchCurrentUser,
				retry: false,
				staleTime: 1000 * 60 * 5, // 5 minutes
			});
		} catch (_error) {
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
