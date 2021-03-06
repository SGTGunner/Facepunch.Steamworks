﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks.Data;

using QueryType = Steamworks.Ugc.Query;

namespace Steamworks.Ugc
{
	public struct Query
	{
		UgcType matchingType;
		UGCQuery queryType;
		AppId consumerApp;
		AppId creatorApp;

		public Query( UgcType type ) : this()
		{
			matchingType = type;
		}

		public static Query All => new Query( UgcType.All );
		public static Query Items => new Query( UgcType.Items );
		public static Query ItemsMtx => new Query( UgcType.Items_Mtx );
		public static Query ItemsReadyToUse => new Query( UgcType.Items_ReadyToUse );
		public static Query Collections => new Query( UgcType.Collections );
		public static Query Artwork => new Query( UgcType.Artwork );
		public static Query Videos => new Query( UgcType.Videos );
		public static Query Screenshots => new Query( UgcType.Screenshots );
		public static Query AllGuides => new Query( UgcType.AllGuides );
		public static Query WebGuides => new Query( UgcType.WebGuides );
		public static Query IntegratedGuides => new Query( UgcType.IntegratedGuides );
		public static Query UsableInGame => new Query( UgcType.UsableInGame );
		public static Query ControllerBindings => new Query( UgcType.ControllerBindings );
		public static Query GameManagedItems => new Query( UgcType.GameManagedItems );


		public Query RankedByVote() { queryType = UGCQuery.RankedByVote; return this; }
		public Query RankedByPublicationDate() { queryType = UGCQuery.RankedByPublicationDate; return this; }
		public Query RankedByAcceptanceDate() { queryType = UGCQuery.AcceptedForGameRankedByAcceptanceDate; return this; }
		public Query RankedByTrend() { queryType = UGCQuery.RankedByTrend; return this; }
		public Query FavoritedByFriends() { queryType = UGCQuery.FavoritedByFriendsRankedByPublicationDate; return this; }
		public Query CreatedByFriends() { queryType = UGCQuery.CreatedByFriendsRankedByPublicationDate; return this; }
		public Query RankedByNumTimesReported() { queryType = UGCQuery.RankedByNumTimesReported; return this; }
		public Query CreatedByFollowedUsers() { queryType = UGCQuery.CreatedByFollowedUsersRankedByPublicationDate; return this; }
		public Query NotYetRated() { queryType = UGCQuery.NotYetRated; return this; }
		public Query RankedByTotalVotesAsc() { queryType = UGCQuery.RankedByTotalVotesAsc; return this; }
		public Query RankedByVotesUp() { queryType = UGCQuery.RankedByVotesUp; return this; }
		public Query RankedByTextSearch() { queryType = UGCQuery.RankedByTextSearch; return this; }
		public Query RankedByTotalUniqueSubscriptions() { queryType = UGCQuery.RankedByTotalUniqueSubscriptions; return this; }
		public Query RankedByPlaytimeTrend() { queryType = UGCQuery.RankedByPlaytimeTrend; return this; }
		public Query RankedByTotalPlaytime() { queryType = UGCQuery.RankedByTotalPlaytime; return this; }
		public Query RankedByAveragePlaytimeTrend() { queryType = UGCQuery.RankedByAveragePlaytimeTrend; return this; }
		public Query RankedByLifetimeAveragePlaytime() { queryType = UGCQuery.RankedByLifetimeAveragePlaytime; return this; }
		public Query RankedByPlaytimeSessionsTrend() { queryType = UGCQuery.RankedByPlaytimeSessionsTrend; return this; }
		public Query RankedByLifetimePlaytimeSessions() { queryType = UGCQuery.RankedByLifetimePlaytimeSessions; return this; }


		public async Task<ResultPage?> GetPageAsync( int page )
		{
			if ( page <= 0 ) throw new System.Exception( "page should be > 0" );

			if ( consumerApp == 0 ) consumerApp = SteamClient.AppId;
			if ( creatorApp == 0 ) creatorApp = consumerApp;

			UGCQueryHandle_t handle;
			handle = SteamUGC.Internal.CreateQueryAllUGCRequest1( queryType, matchingType, creatorApp.Value, consumerApp.Value, (uint)page );

			ApplyConstraints( handle );

			var result = await SteamUGC.Internal.SendQueryUGCRequest( handle );
			if ( !result.HasValue )
				return null;

			if ( result.Value.Result != Steamworks.Result.OK )
				return null;

			return new ResultPage
			{
				Handle = result.Value.Handle,
				ResultCount = (int) result.Value.NumResultsReturned,
				TotalCount = (int)result.Value.TotalMatchingResults,
				CachedData = result.Value.CachedData
			};
		}


		#region SharedConstraints
		public QueryType WithType( UgcType type ){ matchingType = type; return this; }
		bool? WantsReturnOnlyIDs;
		public QueryType WithOnlyIDs( bool b ) { WantsReturnOnlyIDs = b; return this; }
		bool? WantsReturnKeyValueTags;
		public QueryType WithKeyValueTag( bool b ) { WantsReturnKeyValueTags = b; return this; }
		bool? WantsReturnLongDescription;
		public QueryType WithLongDescription( bool b ) { WantsReturnLongDescription = b; return this; }
		bool? WantsReturnMetadata;
		public QueryType WithMetadata( bool b ) { WantsReturnMetadata = b; return this; }
		bool? WantsReturnChildren;
		public QueryType WithChildren( bool b ) { WantsReturnChildren = b; return this; }
		bool? WantsReturnAdditionalPreviews;
		public QueryType WithAdditionalPreviews( bool b ) { WantsReturnAdditionalPreviews = b; return this; }
		bool? WantsReturnTotalOnly;
		public QueryType WithTotalOnly( bool b ) { WantsReturnTotalOnly = b; return this; }
		bool? WantsReturnPlaytimeStats;
		public QueryType WithPlaytimeStats( bool b ) { WantsReturnPlaytimeStats = b; return this; }
		int? maxCacheAge;
		public QueryType AllowCachedResponse( int maxSecondsAge ) { maxCacheAge = maxSecondsAge; return this; }
		string language;
		public QueryType InLanguage( string lang ) { language = lang; return this; }

		List<string> requiredTags;
		bool? matchAnyTag;
		List<string> excludedTags;
		Dictionary<string, string> requiredKv;

		/// <summary>
		/// Found items must have at least one of the defined tags
		/// </summary>
		public QueryType MatchAnyTag() { matchAnyTag = true; return this; }

		/// <summary>
		/// Found items must have all defined tags
		/// </summary>
		public QueryType MatchAllTags() { matchAnyTag = false; return this; }

		public QueryType WithTag( string tag )
		{
			if ( requiredTags == null ) requiredTags = new List<string>();
			requiredTags.Add( tag );
			return this;
		}

		public QueryType WithoutTag( string tag )
		{
			if ( excludedTags == null ) excludedTags = new List<string>();
			excludedTags.Add( tag );
			return this;
		}

		void ApplyConstraints( UGCQueryHandle_t handle )
		{
			if ( requiredTags != null )
			{
				foreach ( var tag in requiredTags )
					SteamUGC.Internal.AddRequiredTag( handle, tag );
			}

			if ( excludedTags != null )
			{
				foreach ( var tag in excludedTags )
					SteamUGC.Internal.AddExcludedTag( handle, tag );
			}

			if ( requiredKv != null )
			{
				foreach ( var tag in requiredKv )
					SteamUGC.Internal.AddRequiredKeyValueTag( handle, tag.Key, tag.Value );
			}

			if ( matchAnyTag.HasValue )
			{
				SteamUGC.Internal.SetMatchAnyTag( handle, matchAnyTag.Value );
			}
		}

		#endregion

	}
}