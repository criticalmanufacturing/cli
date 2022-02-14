var gulp = require('gulp'),
  rootUtils = require("@criticalmanufacturing/dev-tasks/root.main"),
  pluginRunSequence = require('run-sequence'),
  pluginYargs = require('yargs').argv,
  _config = require('./.dev-tasks.json'),
  _packagesPath = './src/', //note the ending /
  _dependenciesPath = './dependencies/',
  _framework = _config.framework,
  //NOTE: order matters in these arrays!
  _dependencies = _config.dependencies,
  _packages = _config.packages,
  _apps = [`${_config.webAppPrefix}.web`],
  tasks = null,
  pluginUtil = require('gulp-util');
applyOps = null;

if (typeof _framework === "string" && _framework !== "") {
  tasks = require("./src/" + _framework + "/gulpfile.js")(gulp, _framework);
}
if (_dependencies != null && _dependencies.length > 0) {
  _dependencies.forEach(function (dep) {
    require(_dependenciesPath + dep + "/gulpfile.js")(gulp, dep);
  });
}
if (_packages != null && _packages.length > 0) {
  _packages.forEach(function (pkg) {
    require(_packagesPath + pkg + "/gulpfile.js")(gulp, pkg);
  });
}

//require(`./apps/${_config.webAppPrefix}.web/gulpfile.js`)(gulp, `${_config.webAppPrefix}.web`);

applyOps = function (actions) {
  if (!Array.isArray(actions)) { actions = [actions]; }
  var operations = [];
  var dependencyOperations = [];
  var packageOperations = [];
  if (_dependencies != null && _dependencies.length > 0) {
    _dependencies.forEach(function (dep) {
      actions.forEach(function (action) {
        operations.push(dep + ">" + action);
      });
    });
  }

  actions.forEach(function (action) {
    if (typeof _framework === "string" && _framework !== "") {
      operations.push(_framework + '>' + action);
    }
  })

  _packages.forEach(function (mod) {
    actions.forEach(function (action) {
      operations.push(mod + ">" + action)
    });
  });

  return operations;
};

/*
 * Build all
 */
gulp.task('build', function (callback) {
  if (pluginYargs.server) {
    var isWebAppCompilable = true;
    rootUtils.runOperation(__dirname, _dependencies, _framework, _packages, _apps, "build", callback, typeof _framework === "string" && _framework !== "", isWebAppCompilable);
  } else {
    var ops = []
    if (pluginYargs.parallelBuild !== false) {
      pluginUtil.log(pluginUtil.colors.yellow(`building in parallel`));
      if (Number.isInteger(pluginYargs.parallelBuild)) {
        pluginUtil.log(pluginUtil.colors.yellow(`Using ${pluginYargs.parallelBuild} tasks in parallel`));
        // split in tasks
        var opsToSliceArray = applyOps('build');
        for (i = 0; i < opsToSliceArray.length; i += pluginYargs.parallelBuild) {
          ops.push(opsToSliceArray.slice(i, i + pluginYargs.parallelBuild));
        }
      } else {
        // all the tasks at the same time!
        ops = [applyOps('build')]
      }
    } else {
      ops = applyOps('build');
    }
    // On customized projects we would only require to compile the web if the project defined a framework on their own
    var isWebAppCompilable = _config.isWebAppCompilable;
    if (isWebAppCompilable === true) {
      ops = ops.concat([`${_config.webAppPrefix}.web>build`]);
    }

    if (ops.length > 0 && ops[0].length > 0) {
      ops = ops.concat(callback);
      pluginRunSequence.apply(this, ops);
    } else {
      callback();
    }
  }

});

/*
 * start running the tests
 */
gulp.task('cliTest', function (callback) { 

  let pkgPromises = _packages.map(pkg => {
    return new Promise((resolve, reject) => {
      try {
        execSync("npm run test", {stdio: ['inherit', 'inherit','pipe' ], cwd: `src\\${pkg}`});        
        resolve();
      } catch (e) {
        reject(e);
      }
    })
  });

  Promise.allSettled(pkgPromises).then((results) => {

    let stacks = "";
    for(const result of results){
      if(result.status === "rejected" && typeof(result.reason.stderr) != "undefined" && 
          !result.reason.stderr.toString().includes("Error: No test files found")) {
        stacks = stacks + result.reason.stack;
      }
    }      
    if(stacks !== "") {
      throw new Error(stacks);
    }    
  });
 });

/*
 * Install all
 */
gulp.task('install', function (callback) {

  if (pluginYargs.server) {
    rootUtils.runOperation(__dirname, _dependencies, _framework, _packages, _apps, "install", callback, typeof _framework === "string" && _framework !== "", true);

  } else {
    var ops = applyOps(['install']);

    if (ops.length > 0) {
      ops = ops.concat(callback);
      pluginRunSequence.apply(this, ops);
    } else {
      callback();
    }
  }
});

/*
 * Install and build apps
 */
gulp.task('apps', function (callback) { pluginRunSequence.apply(this, ['build', `${_config.webAppPrefix}.web>build`, callback]); });

/*
 * start serving the web app
 */
gulp.task('start', function (callback) { pluginRunSequence(`${_config.webAppPrefix}.web>start`); });

/*
 * start running the tests
 */
gulp.task('test', function (callback) { pluginRunSequence.apply(this, applyOps('test').concat(callback)); });

/*
 * clean the workspace
 */
gulp.task('clean', function (callback) { pluginRunSequence.apply(this, applyOps('clean').concat(callback)); })

gulp.task('clean-libs', function (callback) { pluginRunSequence.apply(this, applyOps('clean-libs').concat(callback)); })

gulp.task('purge', function (callback) { pluginRunSequence.apply(this, applyOps('purge').concat(callback)); })

gulp.task('watch', function (callback) { pluginRunSequence.apply(this, applyOps('watch').concat(callback)); });

/**
 * i18n generators
 */
gulp.task('create-missing-i18n', function (callback) { pluginRunSequence.apply(this, applyOps('create-missing-i18n').concat(callback)); });
gulp.task('i18n-ts2po', function (callback) { pluginRunSequence.apply(this, applyOps('i18n-ts2po').concat(callback)); });
gulp.task('i18n-po2ts', function (callback) { pluginRunSequence.apply(this, applyOps('i18n-po2ts').concat(callback)); });

/**
 * CI
 */
gulp.task('publish', function (callback) { pluginRunSequence.apply(this, applyOps('publish').concat(callback)); });

gulp.task('check-version', function (callback) { pluginRunSequence.apply(this, applyOps('check-version').concat(callback)); });
gulp.task('set-version', function (callback) { pluginRunSequence.apply(this, applyOps('set-version').concat(callback)); });
gulp.task('npm-dist-tag-add', function (callback) { pluginRunSequence.apply(this, applyOps('npm-dist-tag-add').concat(callback)); });
gulp.task('npm-dist-tag-rm', function (callback) { pluginRunSequence.apply(this, applyOps('npm-dist-tag-rm').concat(callback)); });
gulp.task('npm-dist-tag-ls', function (callback) { pluginRunSequence.apply(this, applyOps('npm-dist-tag-ls').concat(callback)); });
gulp.task('npm-dist-tag-copy-version', function (callback) { pluginRunSequence.apply(this, applyOps('npm-dist-tag-copy-version').concat(callback)); });

gulp.task('bump-version', function (callback) { pluginRunSequence.apply(this, applyOps('bump-version').concat(callback)); });
gulp.task('generate-package-lock', function (callback) { pluginRunSequence.apply(this, applyOps('generate-package-lock').concat(callback)); });

gulp.task('check-circular-imports', function (callback) { pluginRunSequence.apply(this, applyOps('check-circular-imports').concat(callback)); });
