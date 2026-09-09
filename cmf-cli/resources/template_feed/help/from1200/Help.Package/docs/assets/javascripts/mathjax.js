window.MathJax = {
  tex: {
    inlineMath: [["\\(", "\\)"]],
    displayMath: [["\\[", "\\]"]],
    processEscapes: true,
    processEnvironments: true
  },
  options: {
    ignoreHtmlClass: ".*|",
    processHtmlClass: "arithmatex"
  }
};

document$.subscribe(() => {
  setTimeout(() => {
    if (window.MathJax && MathJax.typesetPromise) {
      MathJax.typesetPromise()
        .catch(err => console.error('MathJax render error:', err));
    }
  }, 100);
});

