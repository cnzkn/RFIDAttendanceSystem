import { queryOptions, useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute, Link, redirect } from "@tanstack/react-router";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import { type BackendSection, fetchUserSections } from "@/lib/api";

const sectionsQuery = queryOptions({
	queryKey: ["user-sections"],
	queryFn: fetchUserSections,
});

export const Route = createFileRoute("/courses")({
	component: CoursesComponent,
	loader: ({ context: { queryClient } }) =>
		queryClient.ensureQueryData(sectionsQuery),
});

function CoursesComponent() {
	const { data: sections } = useSuspenseQuery(sectionsQuery);

	return (
		<div className="min-h-screen bg-slate-100 p-4 md:p-12 font-sans">
			<div className="max-w-[1400px] mx-auto">
				{/* --- PAGE HEADER SECTION --- */}
				<div className="flex flex-col md:flex-row justify-between items-end mb-8 border-b-8 border-black pb-4 gap-4">
					<div className="mr-auto md:mr-0">
						<h1 className="text-5xl md:text-7xl font-black text-gray-900 uppercase tracking-tighter leading-none">
							My
							<br />
							Courses
						</h1>
					</div>
					<div className="text-right">
						<div className="font-bold text-xl uppercase tracking-wide">
							Total Sections
						</div>
						<div className="font-mono text-xl md:text-2xl bg-white border-2 border-black px-4 py-1 shadow-[4px_4px_0px_0px_rgba(0,0,0,1)] inline-block">
							{sections.length.toString().padStart(2, "0")}
						</div>
					</div>
				</div>

				{/* --- CONTENT GRID --- */}
				{sections.length === 0 ? (
					<div className="flex flex-col items-center justify-center py-20">
						<div className="text-2xl font-black uppercase tracking-widest text-gray-400 mb-4">
							No Active Courses Found
						</div>
						<Link to="/">
							<BlockyButton variant="neutral">Return to Dashboard</BlockyButton>
						</Link>
					</div>
				) : (
					<div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
						{sections.map((section: BackendSection, index: number) => (
							<CourseCard
								key={`${section.course.code}-${section.section}-${index}`}
								section={section}
							/>
						))}
					</div>
				)}
			</div>
		</div>
	);
}

function CourseCard({ section }: { section: BackendSection }) {
	const variant = "white";

	return (
		<BlockyCard
			variant={variant}
			className="flex flex-col h-full justify-between group"
		>
			<div className="p-6 flex-grow">
				{/* Header: Course Code Badge*/}
				<div className="flex justify-between items-start mb-4">
					<div className="bg-black text-white text-xs font-mono px-3 py-1 uppercase tracking-widest">
						COURSE #{section.course.code}
					</div>
					{/* Status Indicator Dot */}
					<div className="w-3 h-3 bg-green-500 border-2 border-black rounded-none group-hover:animate-pulse" />
				</div>

				{/* Title and Section */}
				<div className="flex items-center gap-2">
					<h2 className="text-3xl font-black uppercase tracking-tight leading-none mb-2 line-clamp-3">
						{section.course.name}
					</h2>

					<div className="font-black text-lg bg-slate-100 px-2 border-2 border-black">
						{section.section}
					</div>
				</div>
			</div>

			{/* Actions Footer */}
			<div className="p-4 border-t-4 border-black bg-slate-50">
				<div className="flex gap-2">
					<Link
						to="/history/$courseCode/$section"
						params={{
							courseCode: section.course.code.toString(),
							section: section.section,
						}}
						preload={false}
						className="w-full"
					>
						<BlockyButton variant="neutral" size="sm" className="w-full py-2">
							Manage Course
						</BlockyButton>
					</Link>
				</div>
			</div>
		</BlockyCard>
	);
}
