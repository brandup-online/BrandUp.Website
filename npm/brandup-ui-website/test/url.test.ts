import { extractHashFromUrl } from "../source/helpers/url";

describe("extractHashFromUrl", () => {
    it("returns null hash when no fragment", () => {
        expect(extractHashFromUrl("/catalog/elki")).toEqual({ url: "/catalog/elki", hash: null });
    });

    it("splits url and hash", () => {
        expect(extractHashFromUrl("/catalog#section")).toEqual({ url: "/catalog", hash: "section" });
    });

    it("keeps query before the hash", () => {
        expect(extractHashFromUrl("/catalog?page=2#section")).toEqual({ url: "/catalog?page=2", hash: "section" });
    });

    it("treats empty fragment as no hash", () => {
        expect(extractHashFromUrl("/catalog#")).toEqual({ url: "/catalog#", hash: null });
    });
});
