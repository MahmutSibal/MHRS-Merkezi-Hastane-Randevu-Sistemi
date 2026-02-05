export function ThemeInitScript() {
  // Runs before hydration to avoid theme flash.
  // Theme values: "system" | "light" | "dark"
  const code = `
(function(){
  try {
    var key = "mhrs_theme";
    var stored = localStorage.getItem(key);
    var theme = stored || "system";
    var prefersDark = false;
    try {
      prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    } catch(e) {}

    var shouldDark = (theme === "dark") || (theme === "system" && prefersDark);
    var root = document.documentElement;
    if (shouldDark) root.classList.add('dark');
    else root.classList.remove('dark');
  } catch(e) {}
})();
`;

  return <script dangerouslySetInnerHTML={{ __html: code }} />;
}
