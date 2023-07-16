export type Ban = {
  id: number;
  sourceID: number;
  sourceName: string;
  sourceRoleplayLevel: string;
  type: string;
  cKey: string;
  bannedOn: Date;
  bannedBy: string;
  reason: string | undefined;
  expires: Date | undefined;
  unbannedBy: string | undefined;
  banID: number | undefined;
  jobs: string[] | undefined;
  banAttributes: string[] | undefined;
  active: boolean;
};
