import { Avatar, MantineNumberSize } from '@mantine/core';
import { ReactComponent as AssistantIcon } from '../assets/assistant.svg';
import { ReactComponent as ClownIcon } from '../assets/clown.svg';
import { ReactComponent as CaptainIcon } from '../assets/captain.svg';
import { ReactComponent as ChefIcon } from '../assets/chef.svg';
import { ReactComponent as EngineerIcon } from '../assets/engineer.svg';
import { ReactComponent as JanitorIcon } from '../assets/janitor.svg';
import { ReactComponent as MimeIcon } from '../assets/mime.svg';
import { ReactComponent as PrisonerIcon } from '../assets/prisoner.svg';
import React from 'react';

type RandomAvatarProps = {
  size?: MantineNumberSize;
  value: string;
};

const PossibleAvatars = [
  AssistantIcon,
  ClownIcon,
  CaptainIcon,
  ChefIcon,
  EngineerIcon,
  JanitorIcon,
  MimeIcon,
  PrisonerIcon,
];

const generateHashFromString = (string: string) => {
  let hash = 0;
  for (let i = 0; i < string.length; i++) {
    const char = string.charCodeAt(i);
    hash = (hash << 5) - hash + char;
  }
  return Math.abs(hash);
};

export const RandomAvatar = React.memo(({ size, value }: RandomAvatarProps) => {
  const SelectedAvatar = PossibleAvatars[generateHashFromString(value) % PossibleAvatars.length];
  return <Avatar size={size}>{<SelectedAvatar viewBox="7 1 17 17" />}</Avatar>;
});
