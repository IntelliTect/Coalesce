module.exports = {
  "plugins": [
    "@babel/plugin-proposal-optional-chaining",
    "@babel/plugin-proposal-nullish-coalescing-operator",
    "@babel/plugin-syntax-dynamic-import"
  ],
  "presets": [
    [
      "@vue/app",
      {
        "useBuiltIns": "entry"
      }
    ]
  ]
}