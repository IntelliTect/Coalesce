<template>
  <footer class="vp-doc intellitect-footer">
    <hr style="margin: 70px 0" />
    <div class="content">
      <a href="https://www.intellitect.com">
        <img class="dark-only" src="/intellitect-text-white.svg" />

        <img class="light-only" src="/intellitect-text-black.svg" />
      </a>
      <p>
        Coalesce is a free and open-source framework created by
        <a href="https://intellitect.com" target="_blank" rel="noreferrer">
          IntelliTect
        </a>
        to fill our desire to create better apps, faster. IntelliTect is a
        high-end software architecture and development consulting firm based in
        Spokane, Washington.
      </p>
      <p>
        If you're looking for help with your software project, whether it be a
        Coalesce application, other technologies, or even just an idea, reach
        out to us at
        <a href="mailto:info@intellitect.com">info@intellitect.com</a> — we'd
        love to start a conversation!
        <a
          href="https://intellitect.com/our-clients/"
          target="_blank"
          rel="noreferrer"
        >
          Our clients
        </a>
        range from Fortune 100 companies to local small businesses and
        non-profits. There is no job too big or small!
      </p>
    </div>
  </footer>
</template>

<style lang="scss">
.intellitect-footer {
  margin: auto;
  text-align: center;
  img {
    max-height: 6ex;
    margin: auto;
  }
  > .content {
    margin: auto;
    max-width: 800px;
  }

  &.page-footer {
    hr {
      margin-bottom: 20px !important;
    }
    font-size: 14px;
    * {
      line-height: 1.75 !important;
    }
  }
}
</style>
