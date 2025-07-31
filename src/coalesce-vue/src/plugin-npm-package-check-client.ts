/// <reference types="vite/client" />

if (import.meta.hot) {
  import.meta.hot.on("coalesce:npm-check-result", (data) => {
    document.getElementById("coalesce-npm-check-result")?.remove();
    if (!data.wasSuccessful) {
      document.body.insertAdjacentHTML(
        "beforeend",
        `
      <div id="coalesce-npm-check-result">
        <style>
          #coalesce-npm-check-result {
            z-index: 999999;
            position: absolute;
            width: 100%;
            top: 0;
            font-family: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, Courier, monospace
          }
          #coalesce-npm-check-result code {
            font-weight: bold;
            border: 1px solid #777;
            border-radius: 6px;
            padding: 2px 4px;
          }
          #coalesce-npm-check-result .packages-table {
            margin-top: 8px;
            border-collapse: collapse;
            width: 100%;
          }
          #coalesce-npm-check-result .packages-table thead {
            font-weight: bold;
          }
          #coalesce-npm-check-result .packages-table td {
            padding: 2px 8px;
            border-left: 1px solid #777;
            border-right: 1px solid #777;
          }
          #coalesce-npm-check-result .packages-table tr {
            border-top: 1px solid #777;
            border-bottom: 1px solid #777;
          }
          #coalesce-npm-check-result .cta-box {
            margin-top: 12px;
            padding: 10px 14px;
            background-color: #8c1b1b;
            color: #fff;
            border-left: 4px solid #ff5252;
            font-weight: 600;
            font-size: 16px;
            border-radius: 4px;
          }
          #coalesce-npm-check-result .cta-box code {
            background-color: #2a2a2a;
            color: #fff;
            padding: 2px 6px;
            border-radius: 4px;
            font-size: 15px;
          }
        </style>
        <div style="
          margin: 20px auto;
          max-width: 640px;
          padding: 8px 16px;
          font-size: 16px;
          border-radius: 16px;
          background: #680b0b;
          color: #cfcfcf;"
        >
          <button style="float: right; padding: 0 8px 4px 8px; font-size: 30px; cursor: pointer" onclick="document.getElementById('coalesce-npm-check-result')?.remove()">&times;</button>
          <b>Coalesce/Vite</b>: ${data.message}
        </div>
      </div>`,
      );
    }
  });
  import.meta.hot.send("coalesce:npm-check");
}
